using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace optymo_record_splitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LabelPath.Visibility = Visibility.Hidden;

            TextboxTimeTolerance.Text = 60.ToString();
            TextboxVolumeTolerance.Text = 30.ToString();
        }

        private void Splitter(string audioFilePath, DateTime startDateTime, int minTimeBetween, double volumeToleranceLimit)
        {
            List<int> uselessTimes = new List<int>();
            List<int> keepedUselessTimes = new List<int>();

            uselessTimes = VolumeAnalyzer(audioFilePath, volumeToleranceLimit);
            keepedUselessTimes = FindStreaks(uselessTimes, minTimeBetween);

            CreateUsefulFiles(audioFilePath, keepedUselessTimes, startDateTime);
        }

        private void CreateUsefulFiles(string audioFilePath, List<int> uselessTimes, DateTime dateTime)
        {
            string date = dateTime.ToString("dd/MM/yyyy").Replace('/','-');
            int timeOffset = TimeToSeconds(dateTime);

            string sourceFilenameWithoutExtension = Path.GetFileNameWithoutExtension(audioFilePath);
            string sourceFileExtension = Path.GetExtension(audioFilePath);
            string outputDirectory = Path.GetDirectoryName(audioFilePath) + @"\" + date + @"\";

            Directory.CreateDirectory(outputDirectory);

            using (Mp3FileReader reader = new Mp3FileReader(audioFilePath))
            {
                int sampleRate = reader.WaveFormat.SampleRate / 1000;
                int frameCount = 1;
                FileStream fs = null;
                bool nextFile = true;
                Mp3Frame frame = reader.ReadNextFrame();

                while (frame != null)
                {
                    if (uselessTimes.Contains(frameCount / sampleRate))
                    {
                        nextFile = true;

                        frameCount++;
                        frame = reader.ReadNextFrame();
                    }
                    else
                    {
                        if (nextFile)
                        {
                            if (fs != null)
                            {
                                fs.Close();
                            }
                            fs = new FileStream(outputDirectory + sourceFilenameWithoutExtension + "_" + FormatSeconds((frameCount / sampleRate)+timeOffset) + sourceFileExtension, FileMode.Create, FileAccess.Write);
                            nextFile = false;
                        }

                        fs.Write(frame.RawData, 0, frame.RawData.Length);
                        frameCount++;
                        frame = reader.ReadNextFrame();
                    }
                }

                fs.Close();
            }
        }

        private void SplitAudioFile(string audioFilePath, int time)
        {
            string strMP3Folder = Path.GetDirectoryName(audioFilePath) + @"\";
            string strMP3SourceFilename = Path.GetFileName(audioFilePath);
            string strMP3OutputFilename1 = Path.GetFileNameWithoutExtension(audioFilePath) + "_1" + Path.GetExtension(audioFilePath);
            string strMP3OutputFilename2 = Path.GetFileNameWithoutExtension(audioFilePath) + "_2" + Path.GetExtension(audioFilePath);

            using (Mp3FileReader reader = new Mp3FileReader(strMP3Folder + strMP3SourceFilename))
            {
                int sampleRate = reader.WaveFormat.SampleRate / 1000;

                int count = 1;
                Mp3Frame mp3Frame = reader.ReadNextFrame();
                FileStream _fs1 = new FileStream(strMP3Folder + strMP3OutputFilename1, FileMode.Create, FileAccess.Write);
                FileStream _fs2 = new FileStream(strMP3Folder + strMP3OutputFilename2, FileMode.Create, FileAccess.Write);

                while (mp3Frame != null)
                {
                    if (count > time * sampleRate) //Value to confirm for every bit rate
                    {
                        while (mp3Frame != null)
                        {
                            _fs2.Write(mp3Frame.RawData, 0, mp3Frame.RawData.Length);
                            count++;
                            mp3Frame = reader.ReadNextFrame();
                        }
                        return;
                    }

                    _fs1.Write(mp3Frame.RawData, 0, mp3Frame.RawData.Length);
                    count++;
                    mp3Frame = reader.ReadNextFrame();
                }

                _fs1.Close();
                _fs2.Close();
            }
        }

        private List<int> VolumeAnalyzer(string audioFilePath, double tolerance)
        {
            List<int> uselessTimes = new List<int>();

            string strMP3Folder = Path.GetDirectoryName(audioFilePath) + @"\";
            string strMP3SourceFilename = Path.GetFileName(audioFilePath);

            using (NAudio.Wave.AudioFileReader waveReader = new NAudio.Wave.AudioFileReader(strMP3Folder + strMP3SourceFilename))
            {
                var samplesPerSecond = waveReader.WaveFormat.SampleRate * waveReader.WaveFormat.Channels;
                var readBuffer = new float[samplesPerSecond];
                int samplesRead;
                int i = 1;
                do
                {
                    samplesRead = waveReader.Read(readBuffer, 0, samplesPerSecond);
                    if (samplesRead == 0) break;
                    var max = readBuffer.Take(samplesRead).Max();

                    if (max > tolerance) //Minimum volume to keep
                    {
                        Console.WriteLine(i-2 + " - value :" + max + "OK");
                    }
                    else
                    {
                        uselessTimes.Add(i-2);
                        Console.WriteLine(i-2 + " - value :" + max + "useless");
                    }

                    i++;
                } while (samplesRead > 0);

                return uselessTimes;
            }
        }

        /// <summary>
        /// uselessTimes not sorted, tolerence in seconds
        /// </summary>
        private List<int> FindStreaks(List<int> uselessTimes, int tolerance)
        {
            List<int> keepedUselessTimes = new List<int>(); //output list
            List<int> temporaryList = new List<int>();

            int y = 0; //y represents the memory of the previous value

            foreach (int x in uselessTimes)
            {
                if (temporaryList.Count == 0) //if empty
                {
                    temporaryList.Add(x);
                }
                else
                {
                    if (x == y + 1) //streak detected
                    {
                        temporaryList.Add(x);
                    }
                    else //end of the current streak
                    {
                        if (temporaryList.Count >= tolerance) //if streak is long enough to be keeped
                        {
                            //save the streak to the output list
                            foreach (int item in temporaryList)
                            {
                                keepedUselessTimes.Add(item);
                            }
                        }
                        else //if the streak is too short to be keeped
                        {
                            temporaryList.Clear();
                        }
                    }
                }

                y = x;
            }

            if (temporaryList.Count != 0)
            {
                if (temporaryList.Count >= tolerance) //if streak is long enough to be keeped
                {
                    //save the streak to the output list
                    foreach (int item in temporaryList)
                    {
                        keepedUselessTimes.Add(item);
                    }
                }
                else //if the streak is too short to be keeped
                {
                    temporaryList.Clear();
                }

            }

            return keepedUselessTimes;
        }

        private string FormatSeconds(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            string str = time.ToString(@"hh\:mm\:ss");

            return str.Replace(':', '.');
        }

        private void ButtonValidate_Click(object sender, RoutedEventArgs e)
        {
            //Check if the fields are correct

            int parsedValue = 24;

            if (!LabelPath.Content.ToString().Contains("/") && !LabelPath.Content.ToString().Contains("\\"))
            {
                MessageBox.Show("Sélectionnez un fichier audio avant de contiuer");
            }
            else if (DatePickerRecord.SelectedDate == null)
            {
                MessageBox.Show("Sélectionnez une date avant de contiuer");
            }
            else if (!int.TryParse(TextboxHour.Text, out parsedValue) || !int.TryParse(TextboxMinute.Text, out parsedValue))
            {
                MessageBox.Show("L'heure indiquée n'est pas valide");
            }
            else if (Convert.ToInt32(TextboxHour.Text) < 0 || Convert.ToInt32(TextboxHour.Text) > 24 || Convert.ToInt32(TextboxMinute.Text) < 0 || Convert.ToInt32(TextboxMinute.Text) > 59)
            {
                MessageBox.Show("L'heure indiquée n'est pas valide");
            }
            else if (!int.TryParse(TextboxTimeTolerance.Text, out parsedValue))
            {
                MessageBox.Show("La valeur de tolérance de découpage minimale n'est pas valide");
            }
            else if (Convert.ToInt32(TextboxTimeTolerance.Text) < 1)
            {
                MessageBox.Show("La valeur de tolérance de découpage minimale n'est pas valide");
            }
            else if (!int.TryParse(TextboxVolumeTolerance.Text, out parsedValue))
            {
                MessageBox.Show("La valeur de tolérance du volume ignoré n'est pas valide");
            }
            else if (Convert.ToInt32(TextboxVolumeTolerance.Text) < 0 || Convert.ToInt32(TextboxVolumeTolerance.Text) > 100)
            {
                MessageBox.Show("La valeur de tolérance du volume ignoré n'est pas valide (elle doit être entre 0 et 100)");
            }
            else
            {
                string audioFilePath = LabelPath.Content.ToString();

                DateTime date = Convert.ToDateTime(DatePickerRecord.SelectedDate);
                TimeSpan time = new TimeSpan(0, Convert.ToInt32(TextboxHour.Text), Convert.ToInt32(TextboxMinute.Text), 0);
                DateTime startDateTime = date.Add(time);

                double volumeToleranceLimit = Convert.ToDouble(TextboxVolumeTolerance.Text) / 100;
                int minTimeBetween = Convert.ToInt32(TextboxTimeTolerance.Text);

                //Start splitter
                Splitter(audioFilePath, startDateTime, minTimeBetween, volumeToleranceLimit);

                MessageBox.Show("Le fichier a bien été découpé");
                ClearScope();
            }
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "MP3 Files (*.mp3)|*.mp3";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                if (LabelPath.Visibility == Visibility.Hidden)
                {
                    LabelPath.Visibility = Visibility.Visible;
                }

                // Open document
                string filename = dlg.FileName;
                LabelPath.Content = filename;
            }
        }

        private int TimeToSeconds(DateTime dateTime)
        {
            TimeSpan ts = dateTime.TimeOfDay;

            return Convert.ToInt32(ts.TotalSeconds);
        }

        private void ClearScope()
        {
            LabelPath.Content = null;
            DatePickerRecord.SelectedDate = null;
            TextboxHour.Text = "";
            TextboxMinute.Text = "";
            TextboxTimeTolerance.Text = 60.ToString();
            TextboxVolumeTolerance.Text = 30.ToString();
        }
    }
}
