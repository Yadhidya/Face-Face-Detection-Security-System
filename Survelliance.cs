using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;

namespace Face_Detection_Security_System
{
    public partial class Survelliance : Form
    {
        #region Variables
        FilterInfoCollection filter;
        VideoCaptureDevice device;
        LBPHFaceRecognizer model1;
        LBPHFaceRecognizer model2;
        int itr = 0;
        //EigenFaceRecognizer model2;
        //FisherFaceRecognizer model3;
        static readonly CascadeClassifier cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");

        #endregion
        public Survelliance()
        {
            InitializeComponent();
        }

        private void Survelliance_Load(object sender, EventArgs e)
        {
            filter = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in filter)
                choDevice.Items.Add(device.Name);
            choDevice.SelectedIndex = 0;
            device = new VideoCaptureDevice();
        }
        private void btnDetect_Click_1(object sender, EventArgs e)
        {
            model1 = new LBPHFaceRecognizer(1, 8, 8, 8, 1500);
            // model2 = new EigenFaceRecognizer();
            // model3 = new FisherFaceRecognizer();
            string file = Directory.GetCurrentDirectory() + @"\PersonDetails\trainset.yml";
            if (!File.Exists(file))
            {
                MessageBox.Show("Trained Dataset is Empty. Please add some!!");
                Register obj1 = new Register();
                obj1.Show();
                this.Hide();
                this.SuspendLayout();
            }
            else
            {
                model1.Load(file);
                //model2.Load(file);
                //model3.Load(file);
                device = new VideoCaptureDevice(filter[choDevice.SelectedIndex].MonikerString);
                device.NewFrame += Device_NewFrame;
                device.Start();
            }
        }

        private void Device_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Image<Bgr, Byte> currentFrame = new Image<Bgr, byte>((Bitmap)eventArgs.Frame.Clone()).Resize(pic.Width, pic.Height, Inter.Cubic);
            Mat grayImage = new Mat();
            CvInvoke.CvtColor(currentFrame, grayImage, ColorConversion.Bgr2Gray);

            //Enhance the image for better result
            CvInvoke.EqualizeHist(grayImage, grayImage);
            Rectangle[] faces = cascadeClassifier.DetectMultiScale(grayImage, 1.1, 3, Size.Empty, Size.Empty);

            // Create font and brush.
            Font drawFont = new Font("Arial", 14);
            SolidBrush drawBrush = new SolidBrush(Color.Yellow);

            // Set format of string.
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.NoClip;
            foreach (Rectangle face in faces)
            {
                CvInvoke.Rectangle(currentFrame, face, new Bgr(Color.Green).MCvScalar, 2);
                Image<Bgr, Byte> resultImage = currentFrame.Convert<Bgr, Byte>();
                resultImage.ROI = face;
                Image<Gray, Byte> grayFaceResult = resultImage.Convert<Gray, Byte>().Resize(200, 200, Inter.Cubic);
                CvInvoke.EqualizeHist(grayFaceResult, grayFaceResult);
                FaceRecognizer.PredictionResult resultLBPH = model1.Predict(grayFaceResult);
                //FaceRecognizer.PredictionResult resultEigen = model2.Predict(grayFaceResult);
                //FaceRecognizer.PredictionResult resultFisher = model3.Predict(grayFaceResult);
                // Debug.WriteLine("Econfidence: " + resultEigen.Distance + " Label: " + resultEigen.Label + "\n");
                // Debug.WriteLine("Fconfidence: " + resultFisher.Distance + " Label: " + resultFisher.Label + "\n");
                Debug.WriteLine("Lconfidence: " + resultLBPH.Distance + " Label: " + resultLBPH.Label + "\n");
                int label = -1;
                /*if (resultFisher.Label != -1 && resultEigen.Label != -1)
                {
                        if (resultEigen.Label == resultFisher.Label)
                            label = resultEigen.Label;
                }
                else
                {
                    label = -1;
                }*/
                if (resultLBPH.Label != -1)
                {
                    label = resultLBPH.Label;
                }
                else label = -1;

                //converting labels into corresponding names
                string lname;
                string path = Directory.GetCurrentDirectory() + @"\TrainedImages\" + label;
                string[] files = Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories);
                var file = files[0];
                lname = file.Split('\\').Last().Split('_')[0];
                var conf = Convert.ToInt32(100 - (100 * resultLBPH.Distance) / 200);
                if (conf >= 76 && label != -1)
                {
                    CvInvoke.PutText(currentFrame, lname + "-" + conf + "%", new Point(face.X - 2, face.Y - 2),
                        FontFace.HersheyComplex, 0.8, new Bgr(Color.Orange).MCvScalar);
                    CvInvoke.Rectangle(currentFrame, face, new Bgr(Color.Green).MCvScalar, 2);
                    DeleteFaceData(label);
                }
                //here results did not found any know faces
                else
                {
                    CvInvoke.PutText(currentFrame, "", new Point(face.X - 2, face.Y - 2),
                        FontFace.HersheyComplex, 1.0, new Bgr(Color.Orange).MCvScalar);
                    CvInvoke.Rectangle(currentFrame, face, new Bgr(Color.Red).MCvScalar, 2);
                }
            }
            pic.Image = currentFrame.Bitmap;
            //Dispose the Current Frame after processing it to reduce the memory consumption.
            if (currentFrame != null)
                currentFrame.Dispose();
        }
        private void Survelliance_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (device.IsRunning)
                device.Stop();
        }

        private void pic_Click(object sender, EventArgs e)
        {

        }

        private void DeleteFaceData(int label)
        {
            /*delete(label.ToString());
            Register reg = new Register();
            string filepath = Directory.GetCurrentDirectory() + @"\PersonDetails\UID.txt";
            List<string> id = new List<string>(File.ReadAllLines(filepath));
            if (id.Count > 0)
            {
                reg.eigenFaceTrainer();
            }
            else
            {
                string path = Directory.GetCurrentDirectory() + @"\PersonDetails\trainset.yml";
                File.Delete(path);
            }*/
            delete(label.ToString());
            string filePath = Directory.GetCurrentDirectory() + @"\PersonDetails\trainset.yml";
            if (File.Exists(filePath))
            {
                // Load the existing training data
                var faceRecognizer = new LBPHFaceRecognizer();
                faceRecognizer.Load(filePath);

                // Rebuild training data without the specified label
                var images = new List<Image<Gray, byte>>();
                var labels = new List<int>();

                string trainImagesPath = Directory.GetCurrentDirectory() + @"\TrainedImages\";
                foreach (var dir in Directory.GetDirectories(trainImagesPath))
                {
                    int dirLabel = int.Parse(Path.GetFileName(dir));
                    if (dirLabel != label)
                    {
                        foreach (var imagePath in Directory.GetFiles(dir, "*.jpg"))
                        {
                            var img = new Image<Gray, byte>(imagePath);
                            images.Add(img);
                            labels.Add(dirLabel);
                        }
                    }
                }

                if (images.Count > 0)
                {
                    faceRecognizer.Train(images.ToArray(), labels.ToArray());
                    faceRecognizer.Save(filePath);
                }
                else
                {
                    File.Delete(filePath); // No data left, delete the file
                }
            }
        }
        public void delete(string uid)
        {
            string filepath = Directory.GetCurrentDirectory() + @"\PersonDetails\UID.txt";
            List<string> id = new List<string>(File.ReadAllLines(filepath));
            if (id.Contains(uid))
                id.Remove(uid);
            using (StreamWriter writer = new StreamWriter(filepath, false))
            {
                for (int i = 0; i < id.Count; i++)
                    writer.WriteLine(id[i]);
            }
            string filepath2 = Directory.GetCurrentDirectory() + @"\PersonDetails\UID_Details.txt";
            string[] lines = File.ReadAllLines(filepath2);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(uid))
                {
                    string[] u_ids = lines[i].Split('*');
                    lines[i] = u_ids[0] + "*" + u_ids[1] + "*" + u_ids[2] + "*" + u_ids[3] + "*" + u_ids[4] + "*" + DateTime.Now.ToString();//u_ids[0]++//.Replace("*-", DateTime.Now.ToString());
                }
            }

            // Write the modified lines back to the file
            File.WriteAllLines(filepath2, lines);
            /*List<string> id2 = new List<string>(File.ReadAllLines(filepath));
            if (id2.Contains(uid))
                id2.Remove(uid);
            using (StreamWriter writer = new StreamWriter(filepath2, false))
            {
                for (int i = 0; i < id2.Count; i++)
                    writer.WriteLine(id2[i]);
            }*/
        }
    }
}
