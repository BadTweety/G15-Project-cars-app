using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Drawing;

namespace ConsoleApplication6
{
    class Program
    {
        static void Main(string[] args)
        {
            int refreshTime = 200;
            slike.readData();
            //slike.printData();
            slike.invert();
            String URL = "http://localhost:8080/crest/v1/api";
            String avto = "";
            String currTime = "";
            String mSpeed = "";
            int fuelPercentage = 0;
            LogitechGSDK.LogiLcdInit("Project cars testing", LogitechGSDK.LOGI_LCD_TYPE_MONO);
            LogitechGSDK.LogiLcdUpdate();
            while (true)
            {
                dynamic js = null;
                using (var webClient = new System.Net.WebClient())
                {

                    try {
                        var json = webClient.DownloadString(URL);
                        js = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        avto = js.vehicleInformation.mCarName;
                        currTime = js.timings.mCurrentTime;
                        mSpeed = js.carState.mSpeed;
                        fuelPercentage = js.carState.mFuelLevel * 100;
                    }
                    catch (Exception e)
                    {
                        LogitechGSDK.LogiLcdMonoSetText(0, "Error!");
                        
                        LogitechGSDK.LogiLcdMonoSetBackground(mergeArrays(slike.bmpFileData,fuelGauge(100)));
                        LogitechGSDK.LogiLcdUpdate();
                        Thread.Sleep(200);
                        continue;
                    }

                }
                //LogitechGSDK.LogiLcdMonoSetText(0, avto);
                //LogitechGSDK.LogiLcdMonoSetText(1, currTime);
                //LogitechGSDK.LogiLcdMonoSetText(2, mSpeed);
                //
               

                //LogitechGSDK.LogiLcdMonoSetBackground(slike.bmpFileData);
                LogitechGSDK.LogiLcdMonoSetBackground(mergeArrays(slike.bmpFileData, fuelGauge(fuelPercentage)));
                LogitechGSDK.LogiLcdUpdate();
                Thread.Sleep(refreshTime);


            }
        }
        // Merge two arrays into one, needs improvement
        static byte[] mergeArrays(byte[] array1, byte[] array2){
            byte[] returnArray = new byte[array1.Length];
            Array.Copy(array1, returnArray,array1.Length);
            for (int i = 0; i < array1.Length; i++){
                if (array2[i] == 255){
                    returnArray[i] = 255;
                }
            }
            return returnArray;
        }

        // Draw the fuel gauge
        static byte[] fuelGauge(int percentage){                    
            byte[] screenArray = new byte[6880];
            int startX = 16;
            int startY = 34;
            int width = 5;
            for(int i = 0; i < percentage; i++){   
                for(int j = 0; j < width; j++) {
                    screenArray[startY * LogitechGSDK.LOGI_LCD_MONO_WIDTH + startX+i + j* LogitechGSDK.LOGI_LCD_MONO_WIDTH] = 255;
                }
            }
            return screenArray;     
        }


    }

    public class slike
    {
        static Bitmap main = (Bitmap)Bitmap.FromFile("main.bmp");
        public static byte[] bmpFileData = null;
        public static byte[] bitVaues = null;

        public static void readData()
        {
            Rectangle rect = new Rectangle(0, 0, main.Width, main.Height);
            System.Drawing.Imaging.BitmapData bmpData = null;

            int stride = 0;
            try
            {
                bmpData = main.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, main.PixelFormat);
                IntPtr ptr = bmpData.Scan0;
                stride = bmpData.Stride;
                int bytes = bmpData.Stride * main.Height;
                bmpFileData = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(ptr, bmpFileData, 0, bytes);
            }
            finally
            {
                if (bmpData != null)
                    main.UnlockBits(bmpData);
            }

            //using (var ms = new System.IO.MemoryStream())
            //{
            //    main.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            //    bmpFileData = ms.ToArray();
            //}

        }

        public static void printData()
        {
            for (int i = 0; i <bmpFileData.Length; i++)
            {
                Console.Write(bmpFileData[i]);
                Console.Write(" ");
            }
            Console.Write(bmpFileData.Length);
        }

        public static void invert()
        {
            for(int i=0; i< bmpFileData.Length; i++)
            {
                if (bmpFileData[i] == 255)
                {
                    bmpFileData[i] = 0;
                }
                else
                {
                    bmpFileData[i] = 255;
                }
            }

        }

      
    }

    public class LogitechGSDK
    {
        //LCD SDK
        public const int LOGI_LCD_COLOR_BUTTON_LEFT = (0x00000100);
        public const int LOGI_LCD_COLOR_BUTTON_RIGHT = (0x00000200);
        public const int LOGI_LCD_COLOR_BUTTON_OK = (0x00000400);
        public const int LOGI_LCD_COLOR_BUTTON_CANCEL = (0x00000800);
        public const int LOGI_LCD_COLOR_BUTTON_UP = (0x00001000);
        public const int LOGI_LCD_COLOR_BUTTON_DOWN = (0x00002000);
        public const int LOGI_LCD_COLOR_BUTTON_MENU = (0x00004000);
        public const int LOGI_LCD_MONO_BUTTON_0 = (0x00000001);
        public const int LOGI_LCD_MONO_BUTTON_1 = (0x00000002);
        public const int LOGI_LCD_MONO_BUTTON_2 = (0x00000004);
        public const int LOGI_LCD_MONO_BUTTON_3 = (0x00000008);
        public const int LOGI_LCD_MONO_WIDTH = 160;
        public const int LOGI_LCD_MONO_HEIGHT = 43;
        public const int LOGI_LCD_COLOR_WIDTH   = 320;
        public const int LOGI_LCD_COLOR_HEIGHT  = 240;
        public const int LOGI_LCD_TYPE_MONO = (0x00000001);
        public const int LOGI_LCD_TYPE_COLOR = (0x00000002);

        [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode, CallingConvention= CallingConvention.Cdecl)]
        public static extern bool LogiLcdInit(String friendlyName, int lcdType);

        [DllImport("LogitechLcdEnginesWrapper", CharSet=CharSet.Unicode,CallingConvention=CallingConvention.Cdecl)]
        public static extern bool LogiLcdIsConnected(int lcdType);

        [DllImport("LogitechLcdEnginesWrapper", CharSet= CharSet.Unicode,CallingConvention=CallingConvention.Cdecl)]
        public static extern bool LogiLcdIsButtonPressed(int button);

        [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogiLcdUpdate();

        [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogiLcdShutdown();

        [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLcdMonoSetBackground(byte [] monoBitmap);

        [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLcdMonoSetText(int lineNumber, String text);

        [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLcdColorSetBackground(byte [] colorBitmap);

        [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLcdColorSetTitle(String text, int red , int green,int blue);

        [DllImport("LogitechLcdEnginesWrapper", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLcdColorSetText(int lineNumber, String text, int red, int green, int blue);



    }

    }
