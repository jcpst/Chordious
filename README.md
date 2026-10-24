![Chordious Banner](./.github/banner.png)

# Chordious #

[![CI Build](https://github.com/jonthysell/Chordious/actions/workflows/ci.yml/badge.svg)](https://github.com/jonthysell/Chordious/actions/workflows/ci.yml)

Chordious is a fretboard diagram generator for fretted stringed instruments like the ukulele, mandolin, and guitar.

**Official Website:** http://chordious.com/

See http://chordious.com/download/ for download and installation details of the official Windows release.

## Running on macOS and Linux ##

`src/Chordious.Avalonia` is a cross-platform front end built with [Avalonia](https://avaloniaui.net/). It shares `Chordious.Core` and `Chordious.Core.ViewModel` with the Windows (WPF) app, so features and the user config file format are the same. With the [.NET SDK](https://dotnet.microsoft.com/download) (8 or later) installed:

```sh
dotnet run --project src/Chordious.Avalonia
```

To build a standalone `Chordious.app` for macOS (no .NET install needed to run it) into `bld/`:

```sh
scripts/build-macos-app.sh            # this Mac's architecture, or pass osx-arm64 / osx-x64
```

The user config is stored in `~/Library/Application Support/Chordious` on macOS and `~/.local/share/Chordious` on Linux. Differences from the Windows app: there's no auto-update, diagrams can't be exported as GIF, and keyboard shortcuts use Cmd as well as Ctrl on macOS.

## Features ##

* Create clean, scalable, fretboard diagrams
* Extensive styling options
* Find chords for all your instruments and tunings with the Chord Finder
* Find scales for all your instruments and tunings with the Scale Finder
* Save and maintain your collections of diagrams in the Diagram Library
* Import your Classic Chordious ChordLine documents
* Export your diagrams as SVG, JPG, or PNG images
* Rich Diagram Editor
* Automatic updates

Chordious is open-source under the MIT license.

Chordious proudly depends on the following open-source projects:

* [MVVM Toolkit](https://github.com/CommunityToolkit/dotnet) for MVVM infrastructure
* [SVG.NET](https://github.com/vvvv/SVG) for rendering diagrams
* [Wpf Extended Toolkit](https://github.com/dotnetprojects/WpfExtendedToolkit) for additional controls

Copyright (c) 2013-2025 Jon Thysell
