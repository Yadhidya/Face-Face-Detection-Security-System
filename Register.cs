using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Face;
using Emgu.CV.CvEnum;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Drawing.Printing;

namespace Face_Detection_Security_System
{
    public partial class Register : Form
    {
        #region Variables
        private Capture videoCapture = null;
        private Image<Bgr, Byte> currentFrame = null;
        readonly Mat frame = new Mat();
        readonly CascadeClassifier faceCascadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        String name;
        String file;
        String uid;
        static int countFaceAdded;
        List<Image<Gray, Byte>> TrainedFaces = new List<Image<Gray, byte>>();
        List<int> PersonsLabels = new List<int>();
        FaceRecognizer model;
        int i = 0;
        #endregion
        public Register()
        {
           
            InitializeComponent();
            captureLabel.Hide();
            captureProgress.Hide();
            captureBox.Hide();
            textBox1.Enabled = false;

            int id = 0;
            file = Directory.GetCurrentDirectory() + @"\PersonDetails";
            if (!Directory.Exists(file))
                Directory.CreateDirectory(file);
            String filepath1 = Path.Combine(file, "UID.txt");
            String filepath2 = Path.Combine(file, "UID_Details.txt");

            //storing the record of number of faces added in database to use as a label
            String facepath = Path.Combine(file, "FaceCount.txt");
            if (!File.Exists(filepath1) && !File.Exists(filepath2))
            {
                // FileStream fs = File.Create(filepath);
                using (FileStream fs = new FileStream(filepath1, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Dispose();
                }
                using (FileStream fs = new FileStream(filepath2, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Dispose();
                }
                id = 0;
            }
            if (!File.Exists(facepath))
            {
                //FileStream fp = File.Create(facepath);
                using (FileStream fs = new FileStream(facepath, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Dispose();
                }
            }
            try
            {
                string idz = File.ReadAllLines(Path.Combine(file, "UID_Details.txt")).Last();
                string[] u_ids = idz.Split('*');
                id = Convert.ToInt32(u_ids[0]);
                id++;
            }
            catch (Exception e)
            {
                id = 1;
            }
            

            textBox1.Text = id.ToString();

            picCapture.InitialImage = Properties.Resources.download;    //using pic from resources
            picCapture.Image = picCapture.InitialImage;         //initailzing picture box on load
            picCapture.SizeMode = PictureBoxSizeMode.StretchImage;  //stretching pic to fit the window
        }
         
        private void btnAddFace_Click(object sender, EventArgs e)
        {
            string u_Id = textBox1.Text;
            string u_Name = textBox2.Text;
            string u_PhoneNumber = textBox4.Text;
            string u_Address = textBox3.Text;

            //Take input name from user using a prompt box
            uid = Interaction.InputBox("Enter Unique Identification (UID): ","UNIQUE IDENTIFICATION",u_Id);
            name = Interaction.InputBox("Enter Person Name: ", "INPUT NAME", u_Name);

            //create a persondetails folder to store list of added face names
            
            String filepath1 = Path.Combine(file, "UID.txt");
            String filepath2 = Path.Combine(file, "UID_Details.txt");

            //storing the record of number of faces added in database to use as a label
            String facepath = Path.Combine(file, "FaceCount.txt");
            if (!File.Exists(filepath1) && !File.Exists(filepath2))
            {
                // FileStream fs = File.Create(filepath);
                using (FileStream fs = new FileStream(filepath1, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Dispose();
                }
                using (FileStream fs = new FileStream(filepath2, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Dispose();
                }
            }
            if (!File.Exists(facepath)) 
            {
                //FileStream fp = File.Create(facepath);
                using (FileStream fs = new FileStream(facepath, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Dispose();
                }
            }
            using (BinaryReader reader = new BinaryReader(File.Open(facepath, FileMode.Open)))
            {
                try
                {
                    countFaceAdded = reader.ReadInt32();
                }
                catch (Exception)
                {
                    countFaceAdded = -1;
                }
            }
            string[] uids = File.ReadAllLines(filepath1);
            if (uids.Contains(uid))
               MessageBox.Show("UID Already Exists");
            else
            {
                if (countFaceAdded == -1)
                {
                    countFaceAdded = 0;
                }
                countFaceAdded++;
                using (BinaryWriter writer = new BinaryWriter(File.Open(facepath, FileMode.Truncate)))
                {
                    writer.Write(countFaceAdded);
                }
                using (StreamWriter writer = new StreamWriter(filepath1, true))
                {
                    writer.WriteLine(uid);
                }
                using (StreamWriter writer = new StreamWriter(filepath2,true))
                {
                     writer.WriteLine(u_Id+"*"+u_Name+"*"+u_PhoneNumber+"*"+u_Address+"*"+DateTime.Now.ToString()+"*-");
                }
               if (videoCapture != null) videoCapture.Dispose();
               videoCapture = new Capture();
               Application.Idle += ProcessFrame;
            }
        }
        private void ProcessFrame(object sender, EventArgs e)
        {
            int count = 0;
            bool status = false;
            //Capture Video
            if (videoCapture != null && videoCapture.Ptr != IntPtr.Zero)
            {
                status = false;
                videoCapture.Retrieve(frame, 0);
                currentFrame = frame.ToImage<Bgr, Byte>().Resize(picCapture.Width, picCapture.Height, Inter.Cubic);
                //Detect Faces
                //Convert Bgr to Gray Image
                Mat grayImage = new Mat();
                CvInvoke.CvtColor(currentFrame, grayImage, ColorConversion.Bgr2Gray);
                //Enhance the image for better result
                CvInvoke.EqualizeHist(grayImage, grayImage);
                //Create Directory if does not exist
                string path = Directory.GetCurrentDirectory() + @"\TrainedImages\" + uid;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string[] countfiles = Directory.GetFiles(path);

                Rectangle[] faces = faceCascadeClassifier.DetectMultiScale(grayImage, 1.1, 3, Size.Empty, Size.Empty);
                //If faces Detected
                if (faces.Length>0)
                {
                    //Project Square around Detected Face
                    foreach (var face in faces)
                    {
                        CvInvoke.Rectangle(currentFrame, face, new Bgr(Color.Green).MCvScalar, 2);
                        //Add Person 
                        Image<Bgr, Byte> resultImage = currentFrame.Convert<Bgr, Byte>();
                        resultImage.ROI = face;
                        Image<Gray, Byte> grayFaceResult = resultImage.Convert<Gray, Byte>().Resize(200, 200, Inter.Cubic);
                        CvInvoke.EqualizeHist(grayFaceResult, grayFaceResult);
                        captureBox.Image = grayFaceResult.Bitmap;
                        //Save Images to train with a delay of ms
                                //resize the image then saving it
                                resultImage.Resize(200, 200, Inter.Cubic).Save(path + @"\" + name.ToString()+ "_"+ uid.ToString() + "_" + (i++).ToString() + ".jpg");
                        Thread.Sleep(100);
                        foreach (string file in countfiles)
                        {
                            captureLabel.Visible = true;
                            captureProgress.Visible = true;
                            captureBox.Visible = true;
                            if (file.Contains(name))
                            {
                                count++;
                            }
                            if (count == 24)
                            {
                                MessageBox.Show("Records Succesfully Added");
                                videoCapture.Dispose();
                                status = true;
                                captureLabel.Hide();
                                captureProgress.Hide();
                                btnStop_Click(sender,e);
                                break;
                            }                                
                        }
                    }
                }
               // else MessageBox.Show("Multiple Faces Detected");
                //Render the Video Capture into the Picture Box picCapture
                picCapture.Image = currentFrame.Bitmap;
                if (status)
                {
                    //videoCapture.Dispose();
                    return;
                }
            }
            //Dispose the Current Frame after processing it to reduce the memory consumption.
            if (currentFrame != null)
                currentFrame.Dispose();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
           
            picCapture.Image = Properties.Resources.download;
            MessageBox.Show("Face successfully added");
            eigenFaceTrainer();
            this.Close();
        }
        private void fetchFromDirectory()
        {
            TrainedFaces.Clear();
            PersonsLabels.Clear();
            string path = Directory.GetCurrentDirectory() + @"\TrainedImages";
            string[] files = Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                int label = Convert.ToInt32(file.Split('\\').Last().Split('_')[1]);
                PersonsLabels.Add(label);
                Image<Gray, byte> trainedImage = new Image<Gray, byte>(file).Resize(200, 200, Inter.Cubic);
                CvInvoke.EqualizeHist(trainedImage, trainedImage);
                TrainedFaces.Add(trainedImage);
            }
        }
        public void eigenFaceTrainer() 
        {
            fetchFromDirectory();
            Debug.WriteLine("Size of images is: " + TrainedFaces.Count());
            Debug.WriteLine("Size of labels is: "+ PersonsLabels.Count());
            foreach (var name in PersonsLabels)
            {
                Debug.Write(" " + name);
            }
            if (TrainedFaces.Count() > 0)
            {
                file = Directory.GetCurrentDirectory() + @"\PersonDetails";
                model = new LBPHFaceRecognizer(1,8,8,8,1500);
                model.Train(TrainedFaces.ToArray(),PersonsLabels.ToArray());
                String path = Path.Combine(file, "trainset.yml");
                model.Save(path);  // Save trained data in yml file
                MessageBox.Show("Training Faces Completed..");
             
            }
        }

        private void Register_Load(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void captureProgress_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Create document
            PrintDocument _document = new PrintDocument();
            // Add print handler
            _document.PrintPage += new PrintPageEventHandler(Document_PrintPage);
            // Create the dialog to display results
            PrintPreviewDialog _dlg = new PrintPreviewDialog();
            _dlg.ClientSize = new System.Drawing.Size(Width / 2, Height / 2);
            _dlg.Location = new System.Drawing.Point(Left, Top);
            _dlg.MinimumSize = new System.Drawing.Size(375, 250);
            _dlg.UseAntiAlias = true;
            // Setting up our document
            _dlg.Document = _document;
            // Show it
            _dlg.ShowDialog(this);
            // Dispose document
            _document.Dispose();
        }

        private void Document_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Create Bitmap according form size
            Bitmap _bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            // Draw from into Bitmap DC
            this.DrawToBitmap(_bitmap, this.DisplayRectangle);
            // Draw Bitmap into Printer DC
            e.Graphics.DrawImage(_bitmap, 0, 0);
            // No longer deeded - dispose it
            _bitmap.Dispose();
        }
    }
}
