using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Structure;

namespace FaceRecognition
{
    public partial class Form1 : Form
    {
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade faceDetected;
        Image<Bgr, Byte> Frame;
        Image<Bgr, Byte> inputFrame;
        Capture camera;
        Image<Gray, byte> result;
        Image<Gray, byte> TrainedFace = null;
        Image<Gray, byte> grayFace = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> Users = new List<string>();
        int count, numLabels, t;
        string name, names = null;
        string pathImage = null;
        int countString = 0;

        private void Button2_Click(object sender, EventArgs e)
        {
            // сохранение изображения в базу данных

            //count = count + 1;
            //grayFace = camera.QueryGrayFrame().Resize(320,240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            //MCvAvgComp[][] detectedFace = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20)); 
            //foreach (MCvAvgComp f in detectedFace[0])
            //{
            //    TrainedFace = Frame.Copy(f.rect).Convert<Gray, byte>();
            //    break;
            //}
            //TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            //trainingImages.Add(TrainedFace);
            //labels.Add(textBox1.Text);
            //File.WriteAllText(Application.StartupPath + "/Faces/Faces.txt", trainingImages.ToArray().Length.ToString() + ",");
            //for(int i = 1; i < trainingImages.ToArray().Length +1; i++)
            //{
            //    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Faces/face" + i + ".bmp");
            //    File.AppendAllText(Application.StartupPath + "/Faces/Faces.txt", trainingImages.ToArray().Length.ToString() + ",");
            //}
            //textBox1.Clear();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            // загрузка изображения для обработки
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pathImage = openFileDialog.FileName;
                inputFrame = new Image<Bgr, byte>(openFileDialog.FileName);
                imageBox2.Image = inputFrame;
            }
            else
            {
                MessageBox.Show("Can't open file");
            }
            // обработка
            Users.Add("");
            grayFace = inputFrame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach (MCvAvgComp f in facesDetectedNow[0])
            {
                result = inputFrame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                inputFrame.Draw(f.rect, new Bgr(Color.Green), 3);
                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCriterias = new MCvTermCriteria(count, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 1500, ref termCriterias);
                    name = recognizer.Recognize(result);
                    inputFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));
                }
                Users.Add("");
                
            }
        }

        public Form1()
        {
            InitializeComponent();
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                string labelinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
                string[] Labels = labelinf.Split(',');
                numLabels = Convert.ToInt32(Labels[0]);
                count = numLabels;
                string facesLoad;
                for(int i = 1; i < numLabels+1; i++)
                {
                    facesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/Faces.txt"));
                    labels.Add(Labels[i]);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Nothing in database");
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            PutImageInDb(pathImage);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // создание видеопотока
            try
            {

                camera = new Capture();
                camera.QueryFrame();
                Application.Idle += new EventHandler(FrameProcedure);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            // обработка изображения фильтром Хаара
            Users.Add("");
            Frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach(MCvAvgComp f in facesDetectedNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Green), 3);
                if(trainingImages.ToArray().Length !=0)
                {
                    MCvTermCriteria termCriterias = new MCvTermCriteria(count, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 1500, ref termCriterias);
                    name = recognizer.Recognize(result);
                    Frame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));
                }
                Users.Add("");
            }
            imageBox1.Image = Frame;
            names = "";
            Users.Clear();
        }

        private void PutImageInDb(string imageFile)
        {
            byte[] imageData = null;
            FileInfo fileInfo = new FileInfo(imageFile);
            long numBytes = fileInfo.Length;
            FileStream fileStream = new FileStream(imageFile, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            imageData = binaryReader.ReadBytes((int)numBytes);

            // получение расширения
            string imageExtension = Path.GetExtension(imageFile).Replace(".", "").ToLower();

            using (SqlConnection sqlConnection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\vs_project\FaceRecognition\FaceRecognition\FaceDatabase.mdf;Integrated Security=SSPI"))
            {
                
                string commandText = "INSERT INTO FACES (Id, Image, Image_format, Name) VALUES(@Id, @Image, @Image_format, @Name)"; // запрос на вставку
                SqlCommand command = new SqlCommand(commandText, sqlConnection);
                string stmt = "SELECT COUNT(*) FROM dbo.Faces";
                
                using (SqlCommand commandCount = new SqlCommand(stmt, sqlConnection))
                {
                    sqlConnection.Open();
                    countString = (int)commandCount.ExecuteScalar();
                    sqlConnection.Close();
                }
                command.Parameters.AddWithValue("@Id", countString+1);//записывает порядковый номер
                command.Parameters.AddWithValue("@Image", (object)imageData); // записываем само изображение
                command.Parameters.AddWithValue("@Image_format", imageExtension); // записываем расширение изображения
                if(textBox1.Text == "")
                {
                    MessageBox.Show("Введите Имя");
                }
                else
                {
                    command.Parameters.AddWithValue("@Name", textBox1.Text);
                    sqlConnection.Open();
                    command.ExecuteNonQuery();
                    sqlConnection.Close();
                }
               
            }

        }
        private void GetImageBinaryFromDb()
        {
            List<byte[]> iScreen = new List<byte[]>();
            List<string> iScreenFormat = new List<string>();
            using (SqlConnection sqlConnection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\vs_project\FaceRecognition\FaceRecognition\FaceDatabase.mdf;Integrated Security=SSPI"))
            {
                sqlConnection.Open();
                countString = 0;
                SqlCommand sqlCommand = new SqlCommand();
                for (int i = 1; i < countString; i++)
                    sqlCommand.CommandText = @"SELECT [Image], [Image_format] FROM [Faces] WHERE [Id] = " + Convert.ToString(countString);

                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                byte[] iTrimByte = null;
                string iTrimText = null;
                while(sqlDataReader.Read())
                {
                    iTrimByte = (byte[])sqlDataReader["Image"];
                    iScreenFormat.Add(iTrimText);

                }
                sqlConnection.Close();
            }

            byte[] imageData = iScreen[0]; // возвращает массив байт из БД. Так как у нас SQL вернёт одну запись и в ней хранится нужное нам изображение, то из листа берём единственное значение с индексом '0'
            MemoryStream ms = new MemoryStream(imageData);
            Image newImage = Image.FromStream(ms);

            // сохраняем изоражение на диск
            string iImageExtension = iScreenFormat[0]; // получаем расширение текущего изображения хранящееся в БД
            string iImageName = @"C:\result_new" + "." + iImageExtension; // задаём путь сохранения и имя нового изображения
            inputFrame = new Image<Bgr, byte>(iImageName);
            imageBox2.Image = inputFrame;
            //if (iImageExtension == "png") { newImage.Save(iImageName, System.Drawing.Imaging.ImageFormat.Png); }
            //else if (iImageExtension == "jpg" || iImageExtension == "jpeg") { newImage.Save(iImageName, System.Drawing.Imaging.ImageFormat.Jpeg); }
            //else if (iImageExtension == "gif")
            //{
            //    newImage.Save(iImageName, System.Drawing.Imaging.ImageFormat.Gif);
            }
    }
}
