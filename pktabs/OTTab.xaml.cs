using System.Windows.Input;
using System;
using PKHeX.Core;
using static PKHeXMAUI.MainPage;
namespace PKHeXMAUI;

public partial class OTTab : ContentPage
{
    public bool SkipEvent = false;
	public OTTab()
	{
		InitializeComponent();
        htlanguagepicker.ItemsSource = Enum.GetValues(typeof(LanguageID));
       
        CountryPicker.ItemsSource = Util.GetCountryRegionList("countries", GameInfo.CurrentLanguage);
        CountryPicker.ItemDisplayBinding= new Binding("Text");
        DSregionPicker.ItemsSource = datasourcefiltered.ConsoleRegions.ToList();
        DSregionPicker.ItemDisplayBinding = new Binding("Text");
        ICommand refreshCommand = new Command(async () =>
        {

            await applyotinfo(pk);
            OTRefresh.IsRefreshing = false;
        });
        OTRefresh.Command = refreshCommand;
        applyotinfo(pk);
    }

	public async Task applyotinfo(PKM pkm)
	{
        SkipEvent = true;
        eggsprite.IsVisible = pkm.IsEgg;
        countrylabel.IsVisible = false;
        if (pkm.HeldItem > 0)
        {
            itemsprite.Source = itemspriteurl;
            itemsprite.IsVisible = true;
        }
        else
            itemsprite.IsVisible = false;
        if (pkm.IsShiny)
            shinysparklessprite.IsVisible = true;
        else
            shinysparklessprite.IsVisible = false;
        OTpic.Source = spriteurl;
        SIDdisplay.Text = pkm.TrainerSID7.ToString();
        TIDdisplay.Text = pkm.TrainerTID7.ToString();
        otdisplay.Text = pkm.OT_Name;
        ecdisplay.Text = $"{pkm.EncryptionConstant:X}";
        if (pkm.Generation > 5)
        {
            htname.IsVisible = true;
            HTLabel.IsVisible = true;
            HTcurrentcheck.IsVisible = true;
            HTNameLabel.IsVisible = true;
            
            htname.Text = pkm.HT_Name;
            if (pkm is IHandlerLanguage htl)
            {
                htlanguagelabel.IsVisible = true;
                htlanguagepicker.IsVisible = true;
                htlanguagepicker.SelectedIndex = htl.HT_Language;
            }
            switch (pkm.CurrentHandler)
            {
                case 0: OTcurrentcheck.IsChecked = true; HTcurrentcheck.IsChecked = false; break;
                case 1: HTcurrentcheck.IsChecked = true; OTcurrentcheck.IsChecked = false; break;
            };
        }
        if (pkm is IHomeTrack home)
            trackereditor.Text = home.Tracker.ToString("X16");
        extrabytespicker.Items.Clear();
        for (var i=0;i<pkm.ExtraBytes.Length;i++)
            extrabytespicker.Items.Add($"0x{pkm.ExtraBytes[i]:X2}");
        extrabytespicker.SelectedIndex = 0;
        var offset = Convert.ToInt32((string)extrabytespicker.SelectedItem, 16);
        var value = pkm.Data[offset];
        
        extrabytesvalue.Text = value.ToString();
        otgenderpicker.Source = $"gender_{pkm.OT_Gender}.png";
        if(pk is IRegionOrigin regionOrigin)
        {
            countrylabel.IsVisible = true;
            CountryPicker.IsVisible = true;
            var selectedCountry = Util.GetCountryRegionList("countries", GameInfo.CurrentLanguage).Where(z => z.Value == regionOrigin.Country).FirstOrDefault();
            CountryPicker.SelectedItem = selectedCountry;
            subregionlabel.IsVisible = true;
            subregionPicker.IsVisible = true;
            if (regionOrigin.Country != 0)
                subregionPicker.SelectedItem = Util.GetCountryRegionList($"sr_{selectedCountry.Value:000}", GameInfo.CurrentLanguage).Where(z => z.Value == regionOrigin.Region).FirstOrDefault();
            DSregion.IsVisible = true;
            DSregionPicker.IsVisible = true;
            DSregionPicker.SelectedItem = datasourcefiltered.ConsoleRegions.ToList().Where(z=>z.Value == regionOrigin.ConsoleRegion).FirstOrDefault();

        }
        SkipEvent = false;
    }

    private void applySID(object sender, TextChangedEventArgs e)
    {
        if(SIDdisplay.Text.Length > 0 && !SkipEvent)
        {
            pk.TrainerSID7 = uint.Parse(SIDdisplay.Text);
        }
    }

    private void applyTID(object sender, TextChangedEventArgs e)
    {
        if(TIDdisplay.Text.Length > 0 && !SkipEvent)
        {
            pk.TrainerTID7 = uint.Parse(TIDdisplay.Text);
        }
    }

    private void applyot(object sender, TextChangedEventArgs e)
    {
        if(!SkipEvent)
            pk.OT_Name = otdisplay.Text;
    }

    private void applyec(object sender, TextChangedEventArgs e)
    {
        if (!SkipEvent)
            pk.EncryptionConstant = Convert.ToUInt32(ecdisplay.Text,16);
    }

    private void applyHT(object sender, TextChangedEventArgs e)
    {
        if (!SkipEvent)
            pk.HT_Name = htname.Text;
    }

    private void applyhtlanguage(object sender, EventArgs e)
    {
        if(pk is IHandlerLanguage htl && !SkipEvent)
            htl.HT_Language = (byte)htlanguagepicker.SelectedIndex;
    }

    private void MakeOTCurrent(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipEvent)
            pk.CurrentHandler = 0;
    }

    private void MakeHTCurrent(object sender, CheckedChangedEventArgs e)
    {
        if (!SkipEvent)
            pk.CurrentHandler = 1;
    }

    private void applyotgender(object sender, EventArgs e)
    {
        if (pk.OT_Gender == 0)
        {
            pk.OT_Gender = 1;
            otgenderpicker.Source = "gender_1.png";
        }
        else
        {
            pk.OT_Gender = 0;
            otgenderpicker.Source = "gender_0.png";
        }
    }

    private void refreshOT(object sender, EventArgs e)
    {
        if(pk.Species !=0)
            applyotinfo(pk);
    }

    private void applyhometracker(object sender, TextChangedEventArgs e)
    {
        if (ulong.TryParse(trackereditor.Text, out var result) && !SkipEvent) 
        {
            if (pk is IHomeTrack home)
            {
                home.Tracker = result;
             } 
        }
    }

    private void extrabytestuff(object sender, EventArgs e)
    {
        if (!SkipEvent)
        {
            var offset = Convert.ToInt32((string)extrabytespicker.SelectedItem, 16);
            var value = pk.Data[offset];
            extrabytesvalue.Text = value.ToString();
        }
    }

    private void applyextrabytesvalue(object sender, TextChangedEventArgs e)
    {
        if (!SkipEvent)
        {
            var offset = Convert.ToInt32((string)extrabytespicker.SelectedItem, 16);
            pk.Data[offset] = Convert.ToByte(extrabytesvalue.Text);
        }
    }

    private void applyCountry(object sender, EventArgs e)
    {
        if(pk is IRegionOrigin regionOrigin && !SkipEvent)
        {
            var country = (ComboItem)CountryPicker.SelectedItem;
            regionOrigin.Country = (byte)country.Value;
            if (CountryPicker.SelectedIndex != 0)
            {
                subregionPicker.ItemsSource = Util.GetCountryRegionList($"sr_{country.Value:000}", GameInfo.CurrentLanguage);
                subregionPicker.ItemDisplayBinding = new Binding("Text");
            }
            else
                subregionPicker.Items.Clear();
        }
    }

    private void applySubregion(object sender, EventArgs e)
    {
        if (pk is IRegionOrigin regionOrigin && !SkipEvent)
        {
            var subregion = (ComboItem)CountryPicker.SelectedItem;
            regionOrigin.Region = (byte)subregion.Value;
        }

    }

    private void apply3DSregion(object sender, EventArgs e)
    {
        if (pk is IRegionOrigin regionOrigin && !SkipEvent)
        {
            var subregion = (ComboItem)DSregionPicker.SelectedItem;
            regionOrigin.ConsoleRegion = (byte)subregion.Value;
        }
    }
}