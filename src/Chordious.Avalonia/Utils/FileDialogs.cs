// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Synchronous wrappers around Avalonia's async storage pickers.
    /// </summary>
    public static class FileDialogs
    {
        private static IStorageProvider StorageProvider => TopLevel.GetTopLevel(DialogHost.ActiveWindow)?.StorageProvider;

        public static string PromptForOpenFile(string initialDirectory, params (string Name, string[] Extensions)[] filters)
        {
            IStorageProvider provider = StorageProvider;
            if (provider is null || !provider.CanOpen)
            {
                return null;
            }

            FilePickerOpenOptions options = new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                FileTypeFilter = GetFileTypes(filters),
                SuggestedStartLocation = GetFolder(provider, initialDirectory),
            };

            IReadOnlyList<IStorageFile> result = DialogHost.WaitFor(provider.OpenFilePickerAsync(options));

            return result?.FirstOrDefault()?.TryGetLocalPath();
        }

        public static string PromptForSaveFile(string initialDirectory, string filterName, string extension)
        {
            IStorageProvider provider = StorageProvider;
            if (provider is null || !provider.CanSave)
            {
                return null;
            }

            FilePickerSaveOptions options = new FilePickerSaveOptions()
            {
                DefaultExtension = extension,
                FileTypeChoices = GetFileTypes((filterName, new[] { extension })),
                ShowOverwritePrompt = true,
                SuggestedStartLocation = GetFolder(provider, initialDirectory),
            };

            IStorageFile result = DialogHost.WaitFor(provider.SaveFilePickerAsync(options));

            return result?.TryGetLocalPath();
        }

        public static string PromptForFolder(string initialDirectory)
        {
            IStorageProvider provider = StorageProvider;
            if (provider is null || !provider.CanPickFolder)
            {
                return null;
            }

            FolderPickerOpenOptions options = new FolderPickerOpenOptions()
            {
                AllowMultiple = false,
                SuggestedStartLocation = GetFolder(provider, initialDirectory),
            };

            IReadOnlyList<IStorageFolder> result = DialogHost.WaitFor(provider.OpenFolderPickerAsync(options));

            return result?.FirstOrDefault()?.TryGetLocalPath();
        }

        private static List<FilePickerFileType> GetFileTypes(params (string Name, string[] Extensions)[] filters)
        {
            return filters.Select(f => new FilePickerFileType(f.Name)
            {
                Patterns = f.Extensions.Select(ext => ext == "*" ? "*" : $"*.{ext}").ToList(),
            }).ToList();
        }

        private static IStorageFolder GetFolder(IStorageProvider provider, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return null;
            }

            return DialogHost.WaitFor(provider.TryGetFolderFromPathAsync(path));
        }
    }
}
