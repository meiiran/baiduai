using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using Baidu.Aip.Face;
using BaiduAI.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
namespace BaiduAI
{
    public partial class Form1 : Form
    {
        private string APP_ID = "119297448";
        private string API_KEY = "hS3DqvUb0UGHBHv0VvVtqNNt";
        private string SECRET_KEY = "EknhlBem5MQcb52XMhhyA1Mn6LNiGp18";
        private Face client = null;
        private bool IsStart = false;
        private FaceLocation location = null;
        private FilterInfoCollection videoDevices = null;
        private VideoCaptureDevice videoSource;

        public Form1()
        {
            InitializeComponent();
            axWindowsMediaPlayer1.uiMode = "Invisible";
            client = new Face(API_KEY, SECRET_KEY);
        }

        public string ConvertImageToBase64(Image file)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // 强制用JPEG编码，避免encoder为null
                    file.Save(memoryStream, ImageFormat.Jpeg);
                    byte[] imageBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("图片转Base64失败：" + ex.Message);
                return null;
            }
        }
        private void TestFaceDetect(string imagePath)
        {
            try
            {
                // (与方法一中 testDetectButton_Click 内部的代码相同)
                Image img = Image.FromFile(imagePath);
                string imageBase64 = ConvertImageToBase64(img);

                if (string.IsNullOrEmpty(imageBase64))
                {
                    MessageBox.Show("图片转码失败！");
                    return;
                }

                var options = new Dictionary<string, object>{
        {"max_face_num", 1},
        {"face_fields", "age,qualities,beauty"}
    };

                var result = client.Detect(imageBase64, "BASE64", options);
                MessageBox.Show($"API 响应: {result.ToString()}");

                // 解析结果 (可选)
                try
                {
                    Newtonsoft.Json.Linq.JObject jsonResult = Newtonsoft.Json.Linq.JObject.Parse(result.ToString());
                    if (jsonResult["error_code"].Value<int>() == 0)
                    {
                        var faceList = jsonResult["result"]["face_list"] as Newtonsoft.Json.Linq.JArray;
                        if (faceList != null && faceList.Count > 0)
                        {
                            MessageBox.Show("人脸检测成功！");
                            // ...
                        }
                        else
                        {
                            MessageBox.Show("未检测到人脸。");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"API 错误: {jsonResult["error_msg"]}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"解析结果失败: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生错误: {ex.Message}");
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "D:\\404\\BaiduAI\\PersonImg";
            dialog.Filter = "所有文件|*.*";
            dialog.RestoreDirectory = true;
            dialog.FilterIndex = 1;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string filename = dialog.FileName;
                TestFaceDetect(filename);
                try
                {
                    Image im = Image.FromFile(filename);
                    var image = ConvertImageToBase64(im);
                    if (string.IsNullOrEmpty(image))
                    {
                        MessageBox.Show("图片转码失败，无法识别。");
                        return;
                    }
                    string imageType = "BASE64";
                    var options = new Dictionary<string, object>{
                    {"max_face_num", 2},
                    {"face_field", "age,beauty"},
                    {"face_fields", "age,qualities,beauty"}
                };


                    var result = client.Detect(image, imageType, options);
                    MessageBox.Show(result.ToString()); // 调试用
                    textBox1.Text = result.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("识别图片异常：" + ex.Message);
                }
            }
        }

        public string ReadImg(string img)
        {
            try
            {
                return Convert.ToBase64String(File.ReadAllBytes(img));
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取图片失败：" + ex.Message);
                return null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("请选择要对比的人脸图片");
                return;
            }
            try
            {
                string path1 = textBox2.Text;
                string path2 = textBox3.Text;
                var img1 = ReadImg(path1);
                var img2 = ReadImg(path2);
                if (img1 == null || img2 == null)
                {
                    MessageBox.Show("图片读取失败，无法对比。");
                    return;
                }
                var faces = new JArray
            {
                new JObject
                {
                    {"image", img1},
                    {"image_type", "BASE64"},
                    {"face_type", "LIVE"},
                    {"quality_control", "LOW"},
                    {"liveness_control", "NONE"},
                },
                new JObject
                {
                    {"image", img2},
                    {"image_type", "BASE64"},
                    {"face_type", "LIVE"},
                    {"quality_control", "LOW"},
                    {"liveness_control", "NONE"},
                }
            };
                var result = client.Match(faces);
                textBox1.Text = result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("人脸对比异常：" + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "D:\\404\\BaiduAI";
            dialog.Filter = "所有文件|*.*";
            dialog.RestoreDirectory = true;
            dialog.FilterIndex = 2;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(textBox2.Text))
                {
                    textBox2.Text = dialog.FileName;
                }
                else
                {
                    textBox3.Text = dialog.FileName;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices != null && videoDevices.Count > 0)
            {
                comboBox1.Items.Clear();
                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);
                }
                comboBox1.SelectedIndex = 0;
            }
            videoSourcePlayer1.NewFrame += VideoSourcePlayer1_NewFrame;
            ThreadPool.QueueUserWorkItem(new WaitCallback(p =>
            {
                while (true)
                {
                    IsStart = true;
                    Thread.Sleep(500);
                }
            }));
        }
        private void VideoSourcePlayer1_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                if (IsStart)
                {
                    IsStart = false;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.Detect), image.Clone());
                }
                if (location != null)
                {
                    try
                    {
                        Graphics g = Graphics.FromImage(image);
                        // 明确指定使用 System.Drawing.Point
                        g.DrawLine(new Pen(Color.Black), new System.Drawing.Point(location.left, location.top), new System.Drawing.Point(location.left + location.width, location.top));
                        g.DrawLine(new Pen(Color.Black), new System.Drawing.Point(location.left, location.top), new System.Drawing.Point(location.left, location.top + location.height));
                        g.DrawLine(new Pen(Color.Black), new System.Drawing.Point(location.left, location.top + location.height), new System.Drawing.Point(location.left + location.width, location.top + location.height));
                        g.DrawLine(new Pen(Color.Black), new System.Drawing.Point(location.left + location.width, location.top), new System.Drawing.Point(location.left + location.width, location.top + location.height));
                        g.Dispose();
                    }
                    catch (Exception ex)
                    {
                        ClassLoger.Error("VideoSourcePlayer1_NewFrame", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                ClassLoger.Error("VideoSourcePlayer1_NewFrame", ex);
            }
        }

        private void CameraConn()
        {
            if (comboBox1.Items.Count <= 0)
            {
                MessageBox.Show("请插入视频设备");
                return;
            }
            videoSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
            if (videoSource.VideoCapabilities.Length > 0)
            {
                var desiredResolution = videoSource.VideoCapabilities
                    .OrderByDescending(cap => cap.FrameSize.Width * cap.FrameSize.Height)
                    .FirstOrDefault();
                videoSource.VideoResolution = desiredResolution ?? videoSource.VideoCapabilities[0];
            }
            videoSourcePlayer1.VideoSource = videoSource;
            videoSourcePlayer1.Start();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices != null && videoDevices.Count > 0)
            {
                comboBox1.Items.Clear();
                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);
                }
                comboBox1.SelectedIndex = 0;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count <= 0)
            {
                MessageBox.Show("请插入视频设备");
                return;
            }
            try
            {
                if (videoSourcePlayer1.IsRunning)
                {
                    Bitmap bitmap = videoSourcePlayer1.GetCurrentVideoFrame();
                    if (bitmap != null)
                    {
                        string savePath = GetImagePath();
                        string picName = Path.Combine(savePath, $"{DateTime.Now:yyyyMMdd_HHmmssfff}.jpg");
                        int count = 1;
                        string originalName = picName;
                        while (File.Exists(picName))
                        {
                            picName = Path.Combine(savePath, $"{Path.GetFileNameWithoutExtension(originalName)}_{count}.jpg");
                            count++;
                        }
                        bitmap.Save(picName, ImageFormat.Jpeg);
                        MessageBox.Show($"拍照成功！保存至: {picName}", "拍照结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("无法获取视频帧，请重试");
                    }
                }
                else
                {
                    MessageBox.Show("视频未运行，请先启动摄像头");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"拍照失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetImagePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string personImgPath = Path.Combine(appDataPath, "BaiduAI", "PersonImg");
            if (!Directory.Exists(personImgPath))
            {
                Directory.CreateDirectory(personImgPath);
            }
            return personImgPath;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CameraConn();
        }

        public byte[] Bitmap2Byte(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Jpeg);
                    byte[] data = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(data, 0, Convert.ToInt32(stream.Length));
                    return data;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bitmap转byte[]失败：" + ex.Message);
            }
            return null;
        }

        public void Detect(object image)
        {
            try
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ageText.Text = "";
                        textBox4.Text = "";
                    });
                }
                if (image != null && image is Bitmap)
                {
                    Bitmap img = (Bitmap)image;
                    var imgByte = Bitmap2Byte(img);
                    if (imgByte == null)
                    {
                        if (this.IsHandleCreated && !this.IsDisposed)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                MessageBox.Show("图片编码失败，无法检测。");
                            });
                        }
                        return;
                    }
                    Image im = img;
                    string image1 = ConvertImageToBase64(im);
                    if (string.IsNullOrEmpty(image1))
                    {
                        if (this.IsHandleCreated && !this.IsDisposed)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                MessageBox.Show("图片转码失败，无法检测。");
                            });
                        }
                        return;
                    }
                    string imageType = "BASE64";
                    var options = new Dictionary<string, object>{
                {"max_face_num", 2},
                {"face_fields", "age,qualities,beauty"}
            };
                    var result = client.Detect(image1, imageType, options);
                    JObject jsonResult = JObject.Parse(result.ToString());

                    // 检查API调用是否成功
                    if (jsonResult["error_code"]?.Value<int>() != 0 ||
                        jsonResult["result"] == null ||
                        jsonResult["result"]["face_list"] == null ||
                        jsonResult["result"]["face_list"].Count() == 0)
                    {
                        if (this.IsHandleCreated && !this.IsDisposed)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                ageText.Text = "";
                                textBox4.Text = "未检测到人脸";
                            });
                        }
                        return;
                    }

                    // 获取第一个人脸信息
                    var faceInfo = jsonResult["result"]["face_list"][0];

                    // 安全获取年龄值
                    int? age = faceInfo["age"]?.Value<int>();

                    // 安全获取位置信息
                    var locationObj = faceInfo["location"];
                    FaceLocation faceLocation = null;
                    if (locationObj != null)
                    {
                        try
                        {
                            faceLocation = locationObj.ToObject<FaceLocation>();
                        }
                        catch (Exception ex)
                        {
                            ClassLoger.Error("位置信息转换失败", ex);
                        }
                    }

                    // 获取质量信息用于显示建议
                    var qualities = faceInfo["qualities"];
                    var angle = faceInfo["angle"];

                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            // 确保年龄值有效
                            ageText.Text = age.HasValue ? age.Value.ToString() : "未知";

                            // 更新位置信息用于绘制框
                            location = faceLocation;

                            StringBuilder sb = new StringBuilder();

                            // 安全检查质量信息
                            if (qualities != null)
                            {
                                if (qualities["blur"] != null && qualities["blur"].Value<double>() >= 0.7)
                                    sb.AppendLine("人脸过于模糊");
                                if (qualities["completeness"] != null && qualities["completeness"].Value<double>() >= 0.4)
                                    sb.AppendLine("人脸不完整");
                                if (qualities["illumination"] != null && qualities["illumination"].Value<double>() <= 40)
                                    sb.AppendLine("灯光光线质量不好");

                                var occlusion = qualities["occlusion"];
                                if (occlusion != null)
                                {
                                    if (occlusion["left_cheek"] != null && occlusion["left_cheek"].Value<double>() >= 0.8)
                                        sb.AppendLine("左脸颊不清晰");
                                    if (occlusion["left_eye"] != null && occlusion["left_eye"].Value<double>() >= 0.6)
                                        sb.AppendLine("左眼不清晰");
                                    if (occlusion["mouth"] != null && occlusion["mouth"].Value<double>() >= 0.7)
                                        sb.AppendLine("嘴巴不清晰");
                                    if (occlusion["nose"] != null && occlusion["nose"].Value<double>() >= 0.7)
                                        sb.AppendLine("鼻子不清晰");
                                    if (occlusion["right_cheek"] != null && occlusion["right_cheek"].Value<double>() >= 0.8)
                                        sb.AppendLine("右脸颊不清晰");
                                    if (occlusion["right_eye"] != null && occlusion["right_eye"].Value<double>() >= 0.6)
                                        sb.AppendLine("右眼不清晰");
                                    if (occlusion["chin"] != null && occlusion["chin"].Value<double>() >= 0.6)
                                        sb.AppendLine("下巴不清晰");
                                }
                            }

                            // 安全检查角度信息
                            if (angle != null)
                            {
                                if (angle["pitch"] != null && angle["pitch"].Value<double>() >= 20)
                                    sb.AppendLine("俯视角度太大");
                                if (angle["roll"] != null && angle["roll"].Value<double>() >= 20)
                                    sb.AppendLine("脸部应该放正");
                                if (angle["yaw"] != null && angle["yaw"].Value<double>() >= 20)
                                    sb.AppendLine("脸部应该放正点");
                            }

                            // 安全检查位置高度
                            if (faceLocation != null && faceLocation.height <= 100)
                            {
                                sb.AppendLine("人脸部分过小");
                            }

                            textBox4.Text = sb.Length > 0 ? sb.ToString() : "OK";
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        // 显示更详细的错误信息用于调试
                        MessageBox.Show($"人脸检测异常：{ex.Message}\n\n堆栈跟踪：{ex.StackTrace}");
                    });
                }
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private bool CheckGroupExists(string groupId)
        {
            // 简化处理：直接返回false，让系统自动创建用户组
            // 这样可以避免API方法名不匹配的问题
            return false;
        }

        private bool CreateGroup(string groupId)
        {
            // 简化处理：直接返回true，让百度AI API在UserAdd时自动处理用户组
            // 这样可以避免API方法名不匹配的问题
            return true;
        }
        private async void button7_Click(object sender, EventArgs e)
        {
            string uid = "1";
            string userInfo = textBox6.Text.Trim();
            string groupId = textBox5.Text.Trim();

            // 调试输出
            MessageBox.Show($"输入值 - UID: '{uid}', GroupID: '{groupId}', UserInfo: '{userInfo}'");

            try
            {
                if (videoSourcePlayer1.IsRunning)
                {
                    Bitmap bitmap = videoSourcePlayer1.GetCurrentVideoFrame();
                    if (bitmap == null)
                    {
                        MessageBox.Show("无法获取视频帧");
                        return;
                    }

                    string imgBase64 = ConvertImageToBase64(bitmap);
                    if (string.IsNullOrEmpty(imgBase64))
                    {
                        MessageBox.Show("图片转码失败");
                        return;
                    }

                    // 尝试创建用户组
                    try
                    {
                        var groupResult = client.GroupAdd(groupId, new Dictionary<string, object>());
                        textBox1.Text = "创建用户组成功: " + groupResult;
                    }
                    catch (Exception groupEx)
                    {
                        textBox1.Text = "创建用户组失败: " + groupEx.Message;
                    }

                    // 准备参数
                    var options = new Dictionary<string, object>
            {
                {"action_type", "replace"}
            };

                    // 调用API
                    var result = client.UserAdd(uid, userInfo, groupId, imgBase64, options);

                    // 详细日志
                    string debugInfo = $"API调用参数:\n" +
                                      $"UID: '{uid}' (长度:{uid.Length})\n" +
                                      $"GroupID: '{groupId}' (长度:{groupId.Length})\n" +
                                      $"UserInfo: '{userInfo}' (长度:{userInfo.Length})\n" +
                                      $"Base64长度: {imgBase64.Length}\n" +
                                      $"响应: {result}";

                    textBox1.Text = debugInfo;
                    File.WriteAllText("api_debug.log", debugInfo);

                    // 处理结果
                    if (result.ToString().Contains("error_code"))
                    {
                        MessageBox.Show("注册失败:" + result.ToString());
                    }
                    else
                    {
                        MessageBox.Show("注册成功");
                        SaveUserInfo(uid, groupId);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"注册异常：{ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $"\n内部异常：{ex.InnerException.Message}";
                }
                MessageBox.Show(errorMsg);
            }
        }
        private void SaveUserInfo(string uid, string groupId)
        {
            string userFile = Path.Combine(GetImagePath(), "users.txt");
            File.AppendAllText(userFile, $"{uid},{groupId}\n");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string groupId = textBox5.Text.Trim(); // 使用用户输入的groupid
            string uid = textBox7.Text.Trim();
            if (comboBox1.Items.Count <= 0)
            {
                MessageBox.Show("请插入视频设备");
                return;
            }
            try
            {
                if (videoSourcePlayer1.IsRunning)
                {
                    Bitmap bitmap = videoSourcePlayer1.GetCurrentVideoFrame();
                    if (bitmap == null)
                    {
                        MessageBox.Show("无法获取视频帧");
                        return;
                    }

                    // 转换为Base64字符串
                    string image = ConvertImageToBase64(bitmap);

                    var options = new Dictionary<string, object>{
                {"match_threshold", 70},
                {"quality_control", "NORMAL"},
                {"liveness_control", "LOW"},
                {"max_user_num", 3}
            };

                    // 使用用户输入的groupid
                    var result = client.Search(image, "BASE64", groupId, options);

                    if (result.Value<int>("error_code") == 0)
                    {
                        JArray array = result["result"].Value<JArray>("user_list");
                        textBox7.Text = array[0].Value<string>("user_id");
                        axWindowsMediaPlayer1.URL = "20230522_160638_1.mp3";
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("登录异常：" + ex.Message);
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            if (videoDevices == null || videoDevices.Count == 0)
            {
                return;
            }
            videoSource.Stop();
            videoSourcePlayer1.Stop();
        }
    }
}