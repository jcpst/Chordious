// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Chordious.Core.ViewModel;

using Chordious.Avalonia.Resources;

namespace Chordious.Avalonia
{
    public class OptionsViewModelExtended : OptionsViewModel, IIdle
    {
        #region Backgrounds

        public static string SettingsBackgroundGroupLabel
        {
            get
            {
                return Strings.OptionsSettingsBackgroundGroupLabel;
            }
        }

        public static string SelectedRenderBackgroundLabel
        {
            get
            {
                return Strings.OptionsSelectedRendererBackgroundLabel;
            }
        }

        public static string SelectedRenderBackgroundToolTip
        {
            get
            {
                return Strings.OptionsSelectedRendererBackgroundToolTip;
            }
        }

        public int SelectedRenderBackgroundIndex
        {
            get
            {
                return (int)RenderBackground;
            }
            set
            {
                RenderBackground = (Background)(value);
            }
        }

        public ObservableCollection<string> RenderBackgrounds
        {
            get
            {
                return _renderBackgrounds ??= ImageUtils.GetBackgrounds();
            }
        }
        private ObservableCollection<string> _renderBackgrounds;

        private Background RenderBackground
        {
            get
            {

                if (Enum.TryParse(GetSetting("app.renderbackground"), out Background result))
                {
                    return result;
                }

                return Background.None;
            }
            set
            {
                SetSetting("app.renderbackground", value);
                OnPropertyChanged(nameof(SelectedRenderBackgroundIndex));
            }
        }

        public static string SelectedEditorRenderBackgroundLabel
        {
            get
            {
                return Strings.OptionsSelectedEditorRendererBackgroundLabel;
            }
        }

        public static string SelectedEditorRenderBackgroundToolTip
        {
            get
            {
                return Strings.OptionsSelectedEditorRendererBackgroundToolTip;
            }
        }

        public int SelectedEditorRenderBackgroundIndex
        {
            get
            {
                return (int)EditorRenderBackground;
            }
            set
            {
                EditorRenderBackground = (Background)(value);
            }
        }

        public ObservableCollection<string> EditorRenderBackgrounds
        {
            get
            {
                return _editorRenderBackgrounds ??= ImageUtils.GetBackgrounds();
            }
        }
        private ObservableCollection<string> _editorRenderBackgrounds;

        private Background EditorRenderBackground
        {
            get
            {

                if (Enum.TryParse(GetSetting("diagrameditor.renderbackground"), out Background result))
                {
                    return result;
                }

                return Background.None;
            }
            set
            {
                SetSetting("diagrameditor.renderbackground", value);
                OnPropertyChanged(nameof(SelectedEditorRenderBackgroundIndex));
            }
        }

        #endregion

        #region Integrations

        public static string SettingsIntegrationGroupLabel
        {
            get
            {
                return Strings.OptionsSettingsIntegrationGroupLabel;
            }
        }

        public static string EnhancedCopyLabel
        {
            get
            {
                return Strings.OptionsEnhancedCopyLabel;
            }
        }

        public static string EnhancedCopyToolTip
        {
            get
            {
                return Strings.OptionsEnhancedCopyToolTip;
            }
        }

        public bool EnhancedCopy
        {
            get
            {

                if (bool.TryParse(GetSetting("integration.enhancedcopy"), out bool result))
                {
                    return result;
                }

                return false;
            }
            set
            {
                SetSetting("integration.enhancedcopy", value);
                OnPropertyChanged(nameof(EnhancedCopy));
            }
        }

        public static string OpenTempFolderLabel
        {
            get
            {
                return Strings.OptionsOpenTempFolderLabel;
            }
        }

        public static string OpenTempFolderToolTip
        {
            get
            {
                return Strings.OptionsOpenTempFolderToolTip;
            }
        }

        public RelayCommand OpenTempFolder
        {
            get
            {
                return _openTempFolder ??= new RelayCommand(() =>
                {
                    try
                    {
                        string path = IntegrationUtils.GetTempPathAndCreateIfNecessary();
                        StrongReferenceMessenger.Default.Send(new LaunchUrlMessage(path));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _openTempFolder;

        #endregion

        public OptionsViewModelExtended() : base()
        {
            IsIdle = true;
        }

        public override void RefreshProperties()
        {
            base.RefreshProperties();
            OnPropertyChanged(nameof(SelectedRenderBackgroundIndex));
            OnPropertyChanged(nameof(SelectedEditorRenderBackgroundIndex));
            OnPropertyChanged(nameof(EnhancedCopy));
        }
    }
}
