﻿using Microsoft.Win32;
using SFMFManager;
using SFMFManager.Util;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SFMFLauncher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Manager Manager;

        private ObservableCollection<Mod> OnlineMods;
        private ObservableCollection<Mod> SavedMods;
        private ObservableCollection<Mod> InstalledMods;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            Manager = new Manager();

            OnlineMods = new ObservableCollection<Mod>();
            SavedMods = new ObservableCollection<Mod>();
            InstalledMods = new ObservableCollection<Mod>();

            ICOnlineMods.ItemsSource = OnlineMods;
            ICSavedMods.ItemsSource = SavedMods;
            ICInstalledMods.ItemsSource = InstalledMods;

            InitializeUI();
       }

        private void InitializeUI()
        {
            UpdateStates();
            UpdateAllMods();
        }

        private void UpdateStates()
        {
            BtnToggleFramework.Content = Manager.IsSFMFInstalled() ? "Uninstall SFMF" : "Install SFMF";
            UpdateScoreReportingLabel();
        }

        private void MenuImportLocalMod_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "DLL Files (*.dll)|*.dll";
            openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openDialog.ShowDialog() == true)
                Manager.LoadLocalMod(openDialog.FileName);

            UpdateAllMods();
        }

        private void BtnToggleFramework_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.IsSFMFInstalled())
            {
                Manager.UninstallSFMF();
                BtnToggleFramework.Content = "Install SFMF";
            }
            else
            {
                Manager.InstallSFMF();
                BtnToggleFramework.Content = "Uninstall SFMF";
            }

            UpdateStates();
        }

        private void BtnDownloadMod_Click(object sender, RoutedEventArgs e)
        {
            var mod = ((Button)sender).Tag as Mod;
            Manager.DownloadMod(mod);
            UpdateAllMods();
        }

        private void BtnRemoveMod_Click(object sender, RoutedEventArgs e)
        {
            var mod = ((Button)sender).Tag as Mod;
            Manager.RemoveMod(mod);
            UpdateAllMods();
        }

        private void BtnInstallMod_Click(object sender, RoutedEventArgs e)
        {
            var mod = ((Button)sender).Tag as Mod;
            Manager.InstallMod(mod);
            UpdateAllMods();
        }

        private void BtnUninstallMod_Click(object sender, RoutedEventArgs e)
        {
            var mod = ((Button)sender).Tag as Mod;
            Manager.UninstallMod(mod);
            UpdateAllMods();
        }

        private void UpdateScoreReportingLabel()
        {
            if (Manager.IsScoreReportindEnabled())
            {
                LblScoreReportingWarning.Visibility = Visibility.Hidden;
                return;
            }

            var modsAtFault = Manager.InstalledMods.Where(m => m.DisableScoreReporting).Select(m => m.Name);
            LblScoreReportingWarning.Visibility = Visibility.Visible;
            LblScoreReportingWarning.Content = $"{Strings.DisabledScoreReportingWarning}: {string.Join(", ", modsAtFault)}";
        }

        private void UpdateAllMods()
        {
            BtnToggleFramework.IsEnabled = false;
            UpdateOnlineMods();
            UpdateSavedMods();
            UpdateInstalledMods();
            UpdateScoreReportingLabel();
            BtnToggleFramework.IsEnabled = true;
        }

        private void UpdateOnlineMods()
        {
            OnlineMods.Clear();
            foreach (Mod mod in Manager.OnlineMods)
                // Only show mods that aren't downloaded already.
                if (!Manager.SavedMods.Any(m => m.Download == mod.Download) && !Manager.InstalledMods.Any(m => m.Download == mod.Download))
                    OnlineMods.Add(mod);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OnlineMods"));
        }

        private void UpdateSavedMods()
        {
            SavedMods.Clear();
            foreach (Mod mod in Manager.SavedMods)
                SavedMods.Add(mod);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SavedMods"));
        }

        private void UpdateInstalledMods()
        {
            InstalledMods.Clear();
            foreach (Mod mod in Manager.InstalledMods)
                InstalledMods.Add(mod);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InstalledMods"));
        }

        private static class Strings
        {
            public const string DisabledScoreReportingWarning = "Score reporting is disabled";
        }
    }
}
