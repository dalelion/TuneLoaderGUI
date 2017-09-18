using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using System.IO;

namespace TuneLoaderGUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow () {
            InitializeComponent();

            Directory.CreateDirectory("Tunes");

        }

        String UserInput, SongTitle, Path;

        //add more available characters (){}[]+=*&^%$#@!.<>,?`~
        Regex RGX = new Regex("[^a-zA-Z0-9 -]");

        IEnumerable<String> SearchList;

        private async void B_Download_Click (object sender, RoutedEventArgs e) {

            YoutubeClient YTC = new YoutubeClient();

            UserInput = T_LinkOrSearch.Text;

            T_LinkOrSearch.Clear();

            if (UserInput.ToLower().Contains("youtube.com")) {
                UserInput = YoutubeClient.ParseVideoId(UserInput);
            } else {
                SearchList = await YTC.SearchAsync(UserInput);
                UserInput = SearchList.First();
            }

            VideoInfo VideoInfo = await YTC.GetVideoInfoAsync(UserInput);

            AudioStreamInfo ASI = VideoInfo.AudioStreams.OrderBy(x => x.Bitrate).Last();

            SongTitle = VideoInfo.Title;

            SongTitle = RGX.Replace(SongTitle, "");

            //Path = "E:/Tunes/" + $"{SongTitle}.mp3";

            Path = $"Tunes/{ SongTitle}.mp3";

            using (var Input = await YTC.GetMediaStreamAsync(ASI))
            using (var Out = File.Create(Path))
                await Input.CopyToAsync(Out);

            T_Log.Text = $"{SongTitle} added.\n{T_Log.Text}";

        }
    }
}
