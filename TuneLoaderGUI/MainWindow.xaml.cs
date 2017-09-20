using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace TuneLoaderGUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            Directory.CreateDirectory( "Tunes" );

        }

        String UserInput, SongTitle, Path;

        //add more available characters (){}[]+=*&^%$#@!.<>,?`~
        Regex RGX = new Regex( "[^a-zA-Z0-9 -]" );

        IEnumerable<String> SearchList;

        private void OnClose( object sender, CancelEventArgs e ) {
            //CleanUpFiles( "Tunes", ".mp3" ); //TODO: Delete excess file on exit
        }

        private async void B_Download_Click( object sender, RoutedEventArgs e ) {

            YoutubeClient YTC = new YoutubeClient();

            UserInput = T_LinkOrSearch.Text;

            T_LinkOrSearch.Clear();

            if ( UserInput.ToLower().Contains( "youtube.com" ) ) {
                UserInput = YoutubeClient.ParseVideoId( UserInput );
            } else {
                SearchList = await YTC.SearchAsync( UserInput );
                UserInput = SearchList.First();
            }

            VideoInfo VideoInfo = await YTC.GetVideoInfoAsync( UserInput );

            AudioStreamInfo ASI = VideoInfo.AudioStreams.OrderBy( x => x.Bitrate ).Last();

            var SongTitle = RGX.Replace( VideoInfo.Title, "" );

            var Path = $"Tunes/{ SongTitle }.{ASI.Container.GetFileExtension()}";

            using ( var Input = await YTC.GetMediaStreamAsync( ASI ) )
            using ( var Out = File.Create( Path ) )
                await Input.CopyToAsync( Out );

            ToMP3( Path ).Start();

            T_Log.Text = $"{SongTitle} added.\n{T_Log.Text}";
        }

        private Process ToMP3( String Path ) {
            Process MP3Encoder = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "ffmpeg.exe",
                    Arguments = $"-i \"{ Path }\" -vn -ab 128k -ar 44100 -y \"{ Path.Split( '.' )[0] }.mp3\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };

            MP3Encoder.Exited += ( x, y ) => { File.Delete( Path ); };

            return MP3Encoder;

        }

        private void MP3Encoder_Exited( object sender, EventArgs e ) {
            File.Delete( Path ); //Broken if user goes too fast
        }

        private void CleanUpFiles( String Directory, String Extention ) {
            foreach ( FileInfo FileX in new DirectoryInfo( Directory ).GetFiles() ) {
                if ( FileX.Extension != Extention )
                    FileX.Delete();
            }
        }
    }
}
