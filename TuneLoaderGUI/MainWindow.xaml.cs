using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            DeleteFileType( "Tunes", ".mp3" );
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

            SongTitle = VideoInfo.Title;

            SongTitle = RGX.Replace( SongTitle, "" );

            //Path = "E:/Tunes/" + $"{SongTitle}.mp3";

            Path = $"Tunes/{ SongTitle }.{ASI.Container.GetFileExtension()}";

            using ( var Input = await YTC.GetMediaStreamAsync( ASI ) )
            using ( var Out = File.Create( Path ) )
                await Input.CopyToAsync( Out );

            ToMP3( Path );

            T_Log.Text = $"{SongTitle} added.\n{T_Log.Text}";

        }

        private Process ToMP3( string Path ) {
            return Process.Start( new ProcessStartInfo {
                FileName = "ffmpeg.exe",
                Arguments = $"-i \"{ Path }\" -vn -ab 128k -ar 44100 -y \"{ Path }.mp3\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            } );
        }

        private void DeleteFileType( String Directory, String Extention ) {
            foreach ( FileInfo FileX in new DirectoryInfo( Directory ).GetFiles() ) {
                if ( FileX.Extension != Extention )
                    FileX.Delete();
            }
        }

        private void RenameFile( String Path, String Title, String Extention ) {
            File.Move( $"Tunes/{Title}.{Extention}.mp3", $"Tunes/{Title}.mp3" );
        }

    }
}
