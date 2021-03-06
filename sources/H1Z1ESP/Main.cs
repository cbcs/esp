using System;
using System.Text;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using SlimDX.Direct3D9;
using SlimDX;
using System.Linq;



namespace H1Z1ESP
{
    public partial class Main : Form
    {
        private delegate void AsyncWrite(String Text);
        private delegate void AsyncClear();

        public static Boolean IsRunning = false;

        private Int64 CGameOffset = 0x142ADA390; //0x142CD2E90
        private Int64 GraphicsOffset = 0x142ADA0E8; //0x142CD2BE8

        private Hotkey HotKey;
        private Settings Settings;

        private IntPtr GameWindowHandle;
        private MARGIN GameWindowMargin;
        private FastMemory GameMemory = new FastMemory();
        private RECT GameWindowRect;
        private POINT GameWindowSize;
        private POINT GameWindowCenter;

        public static IniHandler Ini;

        public static Boolean Aiming = false;

        private static Boolean Aimed = false;
        private static ENTITY AimedEntity;

        public static Boolean ShowESP = true;

        public static Boolean ShowPlayers = true;
        public static Boolean ShowAggressive = true;
        public static Boolean ShowAnimals = false;
        public static Boolean ShowContainers = false;
        public static Boolean ShowWeapons = true;
        public static Boolean ShowAmmo = true;
        public static Boolean ShowItems = true;
        public static Boolean ShowVehicles = true;

        public static Boolean HideESPWhenAiming = true;
        public static Boolean HideDead = true;

        public static Boolean BoxedPlayers = true;
        public static Boolean BoxedAggressive = false;
        public static Boolean BoxedAnimals = false;
        public static Boolean BoxedItems = false;
        public static Boolean BoxedVehicles = true;
        public static Boolean Boxed3D = true;

        public static Boolean ShowMap = false;

        public static Boolean ShowRadar = false;
        public static int RadarTransparency = 210;
        public static Boolean RadarPlayers = true;
        public static Boolean RadarAggressive = false;
        public static Boolean RadarAnimals = false;
        public static Boolean RadarVehicles = true;

        public static Boolean ShowEntityLists = true;

        private static Vector2 RadarCenter;

        public static Boolean ShowMapLarge = false;
        public static int MapTransparency = 210;
        private static float map_pos_x;
        private static float map_pos_z;

        public static Boolean ShowPosition = false;
        public static Boolean ShowCities = true;

        public static Boolean TextShadow = true;

        public static POINT TextRegion;
        public static List<ENTITY> Entity = new List<ENTITY>();

        public static Vector3 PlayerPosition = Vector3.Zero;
        public static float player_X;
        public static float player_Y;
        public static float player_Z;
        public static float player_D;

        private static SlimDX.Direct3D9.Device DXDevice;
        private static SlimDX.Direct3D9.Sprite DXSprite;
        private static SlimDX.Direct3D9.Texture DXTextrureMap;
        private static SlimDX.Direct3D9.Texture DXTextrureMapLarge;
        private static SlimDX.Direct3D9.Line DXLine;
        private static SlimDX.Direct3D9.Font DXFont;

        [DllImport("shlwapi.dll")]
        public static extern int ColorHLSToRGB(int H, int L, int S);

        public Main()
        {
            String iniPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
            Ini = new IniHandler(iniPath + @"\Settings.ini");

            //
            if (Ini.IniReadValue("Offsets", "CGame") != String.Empty)
                CGameOffset = Convert.ToInt64(Ini.IniReadValue("Offsets", "CGame"), 16);
            if (Ini.IniReadValue("Offsets", "Graphics") != String.Empty)
                GraphicsOffset = Convert.ToInt64(Ini.IniReadValue("Offsets", "Graphics"), 16);

            //
            if (Ini.IniReadValue("ESP", "ShowPlayers") != String.Empty)
                ShowPlayers = Convert.ToBoolean(Ini.IniReadValue("ESP", "ShowPlayers"));

            if (Ini.IniReadValue("ESP", "ShowAggressive") != String.Empty)
                ShowAggressive = Convert.ToBoolean(Ini.IniReadValue("ESP", "ShowAggressive"));

            if (Ini.IniReadValue("ESP", "ShowAnimals") != String.Empty)
                ShowAnimals = Convert.ToBoolean(Ini.IniReadValue("ESP", "ShowAnimals"));

            if (Ini.IniReadValue("ESP", "ShowContainers") != String.Empty)
                ShowContainers = Convert.ToBoolean(Ini.IniReadValue("ESP", "ShowContainers"));

            if (Ini.IniReadValue("ESP", "ShowWeapons") != String.Empty)
                ShowWeapons = Convert.ToBoolean(Ini.IniReadValue("ESP", "ShowWeapons"));

            if (Ini.IniReadValue("ESP", "ShowAmmo") != String.Empty)
                ShowAmmo = Convert.ToBoolean(Ini.IniReadValue("ESP", "ShowAmmo"));

            if (Ini.IniReadValue("ESP", "ShowItems") != String.Empty)
                ShowItems = Convert.ToBoolean(Ini.IniReadValue("ESP", "ShowItems"));

            if (Ini.IniReadValue("ESP", "ShowVehicles") != String.Empty)
                ShowVehicles = Convert.ToBoolean(Ini.IniReadValue("ESP", "ShowVehicles"));

            //
            if (Ini.IniReadValue("Boxed", "Players") != String.Empty)
                BoxedPlayers = Convert.ToBoolean(Ini.IniReadValue("Boxed", "Players"));

            if (Ini.IniReadValue("Boxed", "Aggressive") != String.Empty)
                BoxedAggressive = Convert.ToBoolean(Ini.IniReadValue("Boxed", "Aggressive"));

            if (Ini.IniReadValue("Boxed", "Animals") != String.Empty)
                BoxedAnimals = Convert.ToBoolean(Ini.IniReadValue("Boxed", "Animals"));

            if (Ini.IniReadValue("Boxed", "Items") != String.Empty)
                BoxedItems = Convert.ToBoolean(Ini.IniReadValue("Boxed", "Items"));

            if (Ini.IniReadValue("Boxed", "Vehicles") != String.Empty)
                BoxedVehicles = Convert.ToBoolean(Ini.IniReadValue("Boxed", "Vehicles"));

            if (Ini.IniReadValue("Boxed", "3D") != String.Empty)
                Boxed3D = Convert.ToBoolean(Ini.IniReadValue("Boxed", "3D"));

            //
            if (Ini.IniReadValue("Misc", "ShowPosition") != String.Empty)
                ShowPosition = Convert.ToBoolean(Ini.IniReadValue("Misc", "ShowPosition"));

            if (Ini.IniReadValue("Misc", "ShowCities") != String.Empty)
                ShowCities = Convert.ToBoolean(Ini.IniReadValue("Misc", "ShowCities"));

            //
            if (Ini.IniReadValue("Misc", "HideDead") != String.Empty)
                HideDead = Convert.ToBoolean(Ini.IniReadValue("Misc", "HideDead"));

            if (Ini.IniReadValue("Misc", "HideESPWhenAiming") != String.Empty)
                HideESPWhenAiming = Convert.ToBoolean(Ini.IniReadValue("Misc", "HideESPWhenAiming"));

            //
            if (Ini.IniReadValue("Map", "LargeMap") != String.Empty)
                ShowMapLarge = Convert.ToBoolean(Ini.IniReadValue("Map", "LargeMap"));

            if (Ini.IniReadValue("Map", "Transparency") != String.Empty)
                MapTransparency = Int32.Parse(Ini.IniReadValue("Map", "Transparency"));

            //
            if (Ini.IniReadValue("Radar", "Show") != String.Empty)
                ShowRadar = Convert.ToBoolean(Ini.IniReadValue("Radar", "Show"));

            if (Ini.IniReadValue("Radar", "Transparency") != String.Empty)
                RadarTransparency = Int32.Parse(Ini.IniReadValue("Radar", "Transparency"));

            if (Ini.IniReadValue("Radar", "Players") != String.Empty)
                RadarPlayers = Convert.ToBoolean(Ini.IniReadValue("Radar", "Players"));

            if (Ini.IniReadValue("Radar", "Aggressive") != String.Empty)
                RadarAggressive = Convert.ToBoolean(Ini.IniReadValue("Radar", "Aggressive"));

            if (Ini.IniReadValue("Radar", "Animals") != String.Empty)
                RadarAnimals = Convert.ToBoolean(Ini.IniReadValue("Radar", "Animals"));

            if (Ini.IniReadValue("Radar", "Vehicles") != String.Empty)
                RadarVehicles = Convert.ToBoolean(Ini.IniReadValue("Radar", "Vehicles"));

            if (Ini.IniReadValue("Misc", "ShowEntityLists") != String.Empty)
                ShowEntityLists = Convert.ToBoolean(Ini.IniReadValue("Misc", "ShowEntityLists"));

            InitializeComponent();
        }

        public static RECT GetWindowRect(IntPtr hWnd)
        {
            RECT lpRect = new RECT();
            Native.GetWindowRect(hWnd, out lpRect);
            return lpRect;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            HotKey = new Hotkey();
            HotKey.enable(this.Handle, Hotkey.Modifiers.None, Keys.Insert);
            HotKey.enable(this.Handle, Hotkey.Modifiers.Alt, Keys.F1);
            HotKey.enable(this.Handle, Hotkey.Modifiers.Alt, Keys.F2);
            HotKey.enable(this.Handle, Hotkey.Modifiers.Alt, Keys.F3);
            HotKey.enable(this.Handle, Hotkey.Modifiers.Alt, Keys.F5);

            Native.SetWindowLong(this.Handle, -20, (IntPtr)((Native.GetWindowLong(this.Handle, -20) ^ 0x80000) ^ 0x20));
            Native.SetLayeredWindowAttributes(this.Handle, 0, 0xff, 2);

            PresentParameters parameters = new SlimDX.Direct3D9.PresentParameters();
            parameters.Windowed = true;
            parameters.SwapEffect = SwapEffect.Discard;
            parameters.BackBufferFormat = Format.A8R8G8B8;
            parameters.BackBufferHeight = this.Height;
            parameters.BackBufferWidth = this.Width;
            parameters.PresentationInterval = PresentInterval.One;

            DXDevice = new SlimDX.Direct3D9.Device(new Direct3D(), 0, DeviceType.Hardware, this.Handle, CreateFlags.HardwareVertexProcessing, new PresentParameters[] { parameters });
            if (System.IO.File.Exists("map_large.png")) DXTextrureMapLarge = SlimDX.Direct3D9.Texture.FromFile(DXDevice, "map_large.png");
            if (System.IO.File.Exists("map.png")) DXTextrureMap = SlimDX.Direct3D9.Texture.FromFile(DXDevice, "map.png");
            DXSprite = new SlimDX.Direct3D9.Sprite(DXDevice);
            DXLine = new SlimDX.Direct3D9.Line(DXDevice);
            DXFont = new SlimDX.Direct3D9.Font(DXDevice, new System.Drawing.Font("Tahoma", 8f));

            if (this.GameMemory.Attach("H1Z1 PlayClient (Live)") == false) { Application.Exit(); return; }
            Thread dxThread = new Thread(new ThreadStart(DoProcess));
            dxThread.IsBackground = true;
            dxThread.Start();

            //Thread aimThread = new Thread(new ThreadStart(DoAiming));
            //aimThread.IsBackground = true;
            //aimThread.Start();            
        }

        private void Main_Paint(object sender, PaintEventArgs e)
        {
            this.GameWindowMargin.Top = 0;
            this.GameWindowMargin.Left = 0;
            this.GameWindowMargin.Right = this.Width;
            this.GameWindowMargin.Bottom = this.Height;
            Native.DwmExtendFrameIntoClientArea(this.Handle, ref this.GameWindowMargin);
        }

        #region RemapValue
        public static float RemapValue(float value, float from1, float to1, float from2, float to2)
        {
            return ((((value - from1) / (to1 - from1)) * (to2 - from2)) + from2);
        }
        #endregion

        #region DrawFilledBox
        public static void DrawFilledBox(float x, float y, float w, float h, Color Color, int alpha = 255)
        {
            Vector2[] vertexList = new Vector2[2];
            DXLine.GLLines = true;
            DXLine.Antialias = false;
            DXLine.Width = w;
            vertexList[0].X = x + (w / 2f);
            vertexList[0].Y = y;
            vertexList[1].X = x + (w / 2f);
            vertexList[1].Y = y + h;
            DXLine.Begin();
            DXLine.Draw(vertexList, Color.FromArgb(alpha, Color.R, Color.G, Color.B));
            DXLine.End();
        }
        #endregion

        #region RotatePoint
        private static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, float angle, bool angleInRadians = false)
        {
            if (!angleInRadians) angle = (float)(angle * 0.017453292519943295);
            float num = (float)Math.Cos((double)angle);
            float num2 = (float)Math.Sin((double)angle);
            Vector2 vector = new Vector2((num * (pointToRotate.X - centerPoint.X)) - (num2 * (pointToRotate.Y - centerPoint.Y)), (num2 * (pointToRotate.X - centerPoint.X)) + (num * (pointToRotate.Y - centerPoint.Y)));
            return (vector + centerPoint);
        }
        #endregion

        #region GetMatrixAxis
        private static Vector3 GetMatrixAxis(Matrix matrix, int i)
        {
            switch (i)
            {
                case 0: return new Vector3(matrix.M11, matrix.M12, matrix.M13);
                case 1: return new Vector3(matrix.M21, matrix.M22, matrix.M23);
                case 2: return new Vector3(matrix.M31, matrix.M32, matrix.M33);
                case 3: return new Vector3(matrix.M41, matrix.M42, matrix.M43);
            }
            return Vector3.Zero;
        }
        #endregion

        #region WorldToScreen
        private bool WorldToScreen(Vector3 vector, out Vector3 screen)
        {
            screen = Vector3.Zero;
            long num = this.GameMemory.ReadInt64(GraphicsOffset);
            long num2 = this.GameMemory.ReadInt64(num + 0x48L);
            long num3 = this.GameMemory.ReadInt64(num2 + 0x20L) + 0x10L;
            Matrix4 matrix = this.GameMemory.ReadMatrix(num3 + 0x1a0L);
            Matrix matrix2 = new Matrix();
            matrix2.M11 = matrix.M11;
            matrix2.M12 = matrix.M12;
            matrix2.M13 = matrix.M13;
            matrix2.M14 = matrix.M14;
            matrix2.M21 = matrix.M21;
            matrix2.M22 = matrix.M22;
            matrix2.M23 = matrix.M23;
            matrix2.M24 = matrix.M24;
            matrix2.M31 = matrix.M31;
            matrix2.M32 = matrix.M32;
            matrix2.M33 = matrix.M33;
            matrix2.M34 = matrix.M34;
            matrix2.M41 = matrix.M41;
            matrix2.M42 = matrix.M42;
            matrix2.M43 = matrix.M43;
            matrix2.M44 = matrix.M44;
            Matrix.Transpose(ref matrix2, out matrix2);
            matrix2.M21 *= -1f;
            matrix2.M22 *= -1f;
            matrix2.M23 *= -1f;
            matrix2.M24 *= -1f;
            float introduced8 = Vector3.Dot(GetMatrixAxis(matrix2, 3), vector);
            float num4 = introduced8 + matrix2.M44;
            if (num4 < 0.098f) return false;
            float introduced9 = Vector3.Dot(GetMatrixAxis(matrix2, 0), vector);
            float num5 = introduced9 + matrix2.M14;
            float introduced10 = Vector3.Dot(GetMatrixAxis(matrix2, 1), vector);
            float num6 = introduced10 + matrix2.M24;
            screen.X = ((((this.GameWindowSize.X - 0x10) / 2) * (1f + (num5 / num4))) + this.GameWindowRect.Left) + 8f;
            screen.Y = ((((this.GameWindowSize.Y - 0x26) / 2) * (1f - (num6 / num4))) + this.GameWindowRect.Top) + 30f;
            return true;
        }
        #endregion

        #region DrawLine
        public static void DrawLine(float x1, float y1, float x2, float y2, float w, Color Color)
        {
            Vector2[] vertexList = new Vector2[] { new Vector2(x1, y1), new Vector2(x2, y2) };
            DXLine.GLLines = true;
            DXLine.Width = w;
            DXLine.Antialias = true;
            DXLine.Begin();
            DXLine.Draw(vertexList, Color);
            DXLine.End();
        }
        #endregion

        #region DrawText
        public static void DrawText(string text, int x, int y, Color color, bool center = false)
        {
            int offset = center ? (text.Length * 5) / 2 : 0;
            if (TextShadow) DXFont.DrawString(null, text, x - offset + 1, y + 1, (Color4)Color.Black);
            DXFont.DrawString(null, text, x - offset, y, (Color4)color);
        }

        public static void DrawText(string text, ref POINT point, Color color)
        {
            if (TextShadow) DXFont.DrawString(null, text, point.X + 1, point.Y + 1, (Color4)Color.Black);
            DXFont.DrawString(null, text, point.X, point.Y, (Color4)color); point.Y += 15;
        }
        #endregion

        #region DrawBox
        public static void DrawBox(float x, float y, float w, float h, float px, Color Color)
        {
            DrawFilledBox(x - (w / 2f), (y + h) - (h / 2f), w, px, Color);
            DrawFilledBox((x - (w / 2f)) - px, y - (h / 2f), px, h, Color);
            DrawFilledBox(x - (w / 2f), (y - px) - (h / 2f), w, px, Color);
            DrawFilledBox((x - (w / 2f)) + w, y - (h / 2f), px, h, Color);
        }
        #endregion

        #region DrawBoxAbs
        public static void DrawBoxAbs(float x, float y, float w, float h, float px, Color Color)
        {
            DrawFilledBox(x, y + h, w, px, Color);
            DrawFilledBox(x - px, y, px, h, Color);
            DrawFilledBox(x, y - px, w, px, Color);
            DrawFilledBox(x + w, y, px, h, Color);
        }
        #endregion


        #region EntityToScreen
        public void EntityToScreen(Vector3 pos, string name, Color color, bool dist, bool line, bool bounds, float boxHeight, float yaw, float pitch)
        {
            string text = name;
            Vector3 dest = Vector3.Zero;
            this.WorldToScreen(pos, out dest);

            double distX = pos.X - player_X;
            double distY = pos.Z - player_Z;
            double distZ = pos.Y - player_Y;
            double a = Math.Sqrt(((distX * distX) + (distY * distY)) + (distZ * distZ));

            if (dest.Y > 0f && dest.X > 0f && dest.Y >= this.GameWindowRect.Top + 20 && dest.X >= this.GameWindowRect.Left && dest.X <= this.GameWindowRect.Right && dest.Y <= this.GameWindowRect.Bottom)
            {
                if (dist)
                {
                    text = text + " [" + Math.Round(a).ToString() + "m]";
                }

                if (line)
                {
                    DrawLine(dest.X, dest.Y, this.GameWindowCenter.X, this.GameWindowCenter.Y, 1f, color);
                }

                if (bounds)
                {
                    if (!Boxed3D || a >= 100.0)
                    {
                        DrawBox(dest.X, dest.Y, 65f / Math.Max((float)(((float)a) / 10f), (float)0.2f), 150f / Math.Max((float)(((float)a) / 10f), (float)0.2f), 1f, color);
                    }
                    else if (a < 100.0)
                    {
                        float wOffset = (boxHeight < 1f) ? boxHeight : 1f;
                        float zOffset = (boxHeight > 1f) ? boxHeight / 2 : 0f;
                        double num5 = pos.Z + ((1.0 * Math.Cos((double)pitch)) * Math.Cos((double)yaw));
                        double num6 = pos.X + ((1.0 * Math.Cos((double)pitch)) * Math.Sin((double)yaw));
                        double num7 = pos.Y + (1.0 * Math.Sin((double)pitch));
                        Vector3 vector = new Vector3((float)num6, (float)num7, (float)num5);
                        Vector3 zero = Vector3.Zero;
                        this.WorldToScreen(vector, out zero);
                        DrawLine(dest.X, dest.Y, zero.X, zero.Y, 1f, color);
                        num5 = pos.Z + (0.5 * Math.Cos(yaw + 0.78539816339744828));
                        num6 = pos.X + (0.5 * Math.Sin(yaw + 0.78539816339744828));
                        Vector3 vector4 = new Vector3((float)num6, pos.Y - zOffset, (float)num5);
                        Vector3 screen = Vector3.Zero;
                        this.WorldToScreen(vector4, out screen);
                        num5 = pos.Z + (0.5 * Math.Cos(yaw + 5.497787143782138));
                        num6 = pos.X + (0.5 * Math.Sin(yaw + 5.497787143782138));
                        Vector3 vector6 = new Vector3((float)num6, pos.Y - zOffset, (float)num5);
                        Vector3 vector7 = Vector3.Zero;
                        this.WorldToScreen(vector6, out vector7);
                        DrawLine(screen.X, screen.Y, vector7.X, vector7.Y, 1f, color);
                        num5 = pos.Z + (0.5 * Math.Cos(yaw + 3.9269908169872414));
                        num6 = pos.X + (0.5 * Math.Sin(yaw + 3.9269908169872414));
                        Vector3 vector8 = new Vector3((float)num6, pos.Y - zOffset, (float)num5);
                        Vector3 vector9 = Vector3.Zero;
                        this.WorldToScreen(vector8, out vector9);
                        DrawLine(vector7.X, vector7.Y, vector9.X, vector9.Y, 1f, color);
                        num5 = pos.Z + (0.5 * Math.Cos(yaw + 2.3561944901923448));
                        num6 = pos.X + (0.5 * Math.Sin(yaw + 2.3561944901923448));
                        Vector3 vector10 = new Vector3((float)num6, pos.Y - zOffset, (float)num5);
                        Vector3 vector11 = Vector3.Zero;
                        this.WorldToScreen(vector10, out vector11);
                        DrawLine(vector9.X, vector9.Y, vector11.X, vector11.Y, 1f, color);
                        DrawLine(vector11.X, vector11.Y, screen.X, screen.Y, 1f, color);
                        Vector3 vector12 = new Vector3(vector4.X, vector4.Y + boxHeight, vector4.Z);
                        Vector3 vector13 = Vector3.Zero;
                        this.WorldToScreen(vector12, out vector13);
                        DrawLine(screen.X, screen.Y, vector13.X, vector13.Y, 1f, color);
                        Vector3 vector14 = new Vector3(vector6.X, vector6.Y + boxHeight, vector6.Z);
                        Vector3 vector15 = Vector3.Zero;
                        this.WorldToScreen(vector14, out vector15);
                        DrawLine(vector13.X, vector13.Y, vector15.X, vector15.Y, 1f, color);
                        DrawLine(vector7.X, vector7.Y, vector15.X, vector15.Y, 1f, color);
                        Vector3 vector16 = new Vector3(vector8.X, vector8.Y + boxHeight, vector8.Z);
                        Vector3 vector17 = Vector3.Zero;
                        this.WorldToScreen(vector16, out vector17);
                        DrawLine(vector15.X, vector15.Y, vector17.X, vector17.Y, 1f, color);
                        DrawLine(vector9.X, vector9.Y, vector17.X, vector17.Y, 1f, color);
                        Vector3 vector18 = new Vector3(vector10.X, vector10.Y + boxHeight, vector10.Z);
                        Vector3 vector19 = Vector3.Zero;
                        this.WorldToScreen(vector18, out vector19);
                        DrawLine(vector17.X, vector17.Y, vector19.X, vector19.Y, 1f, color);
                        DrawLine(vector11.X, vector11.Y, vector19.X, vector19.Y, 1f, color);
                        DrawLine(vector19.X, vector19.Y, vector13.X, vector13.Y, 1f, color);
                    }
                }
                DrawText(text, (int)dest.X, (int)dest.Y - 20, color, true);
            }
        }
        #endregion

        public void DoProcess()
        {
            Main.IsRunning = true;
            this.GameWindowHandle = this.GameMemory.Process.MainWindowHandle;
            Native.SetForegroundWindow(this.GameWindowHandle);

            while (this.GameWindowHandle != IntPtr.Zero && Main.IsRunning && this.GameMemory.IsOpen)
            {
                this.GameWindowRect = GetWindowRect(this.GameWindowHandle);
                this.GameWindowSize.X = this.GameWindowRect.Right - this.GameWindowRect.Left;
                this.GameWindowSize.Y = this.GameWindowRect.Bottom - this.GameWindowRect.Top;
                this.GameWindowCenter.X = this.GameWindowRect.Left + (this.GameWindowSize.X / 2);
                this.GameWindowCenter.Y = this.GameWindowRect.Top + (this.GameWindowSize.Y / 2) + 11;

                DXDevice.Clear(ClearFlags.Target, Color.FromArgb(0, 0, 0, 0), 1f, 0);
                DXDevice.SetRenderState(RenderState.ZEnable, false);
                DXDevice.SetRenderState(RenderState.Lighting, false);
                DXDevice.SetRenderState<Cull>(RenderState.CullMode, Cull.None);
                DXDevice.BeginScene();

                long entityOffset = this.GameMemory.ReadInt64(CGameOffset);
                long playerOffset = this.GameMemory.ReadInt64(entityOffset + 0x11D8);
                player_X = this.GameMemory.ReadFloat(playerOffset + 0x210); //0x200
                player_Y = this.GameMemory.ReadFloat(playerOffset + 0x214); //0x204
                player_Z = this.GameMemory.ReadFloat(playerOffset + 0x218); //0x208
                player_D = this.GameMemory.ReadFloat(playerOffset + 0x240); //0x230

                long posOffset = this.GameMemory.ReadInt64(playerOffset + 0x198); //0x190
                player_X = this.GameMemory.ReadFloat(posOffset + 0x110);
                player_Y = this.GameMemory.ReadFloat(posOffset + 0x114);
                player_Z = this.GameMemory.ReadFloat(posOffset + 0x118);

                PlayerPosition.X = player_X;
                PlayerPosition.Y = player_Y;
                PlayerPosition.Z = player_Z;

                TextRegion = new POINT(this.GameWindowRect.Left + 15, this.GameWindowRect.Top + 35);
                if (ShowPosition)
                {
                    DrawText("Position X: " + player_X.ToString("F1") + " Y: " + player_Y.ToString("F1") + " Z: " + player_Z.ToString("F1"), ref TextRegion, Color.White);
                    DrawText("Direction: " + player_D.ToString("F2"), ref TextRegion, Color.White);
                }

                ShowESP = (!HideESPWhenAiming || Convert.ToBoolean(Native.GetAsyncKeyState(2) & 0x8000) == false);

                Entity.Clear(); Aimed = false;

                int entityCount = this.GameMemory.ReadInt32(entityOffset + 0x688);
                long entityEntry = this.GameMemory.ReadInt64(playerOffset + 0x410); //0x400

                for (int i = 1; i < entityCount; i++)
                {
                    float EntityX = 0;
                    float EntityY = 0;
                    float EntityZ = 0;
                    float EntityYaw = 0;
                    float EntityPitch = 0;
                    float EntitySpeed = 0;

                    int EntityType = this.GameMemory.ReadInt32(entityEntry + 0x5C8); //0x5B0
                    if (EntityType == 0) continue;

                    int EntityId = this.GameMemory.ReadInt32(entityEntry + 0x620); //0x608

                    long _nameEntry = this.GameMemory.ReadInt64(entityEntry + 0x4E0); //0x468
                    String EntityName = this.GameMemory.ReadString(_nameEntry, this.GameMemory.ReadInt32(entityEntry + 0x4E8)); //0x470

                    // Player Position //
                    if (EntityType == 0x04)
                    {
                        EntityX = this.GameMemory.ReadFloat(entityEntry + 0x210); //0x1C0
                        EntityY = this.GameMemory.ReadFloat(entityEntry + 0x214); //0x1C4
                        EntityZ = this.GameMemory.ReadFloat(entityEntry + 0x218); //0x1C8

                        EntitySpeed = this.GameMemory.ReadFloat(entityEntry + 0x2C8); //0x1D8
                        EntityYaw = this.GameMemory.ReadFloat(entityEntry + 0x2E0); //0x1F0
                    }
                    // Vechicle Position //
                    else if (EntityType == 0x11 || EntityType == 0x72 || EntityType == 0x76)
                    {
                        EntityX = this.GameMemory.ReadFloat(entityEntry + 0x250); //0x200
                        EntityY = this.GameMemory.ReadFloat(entityEntry + 0x254);
                        EntityZ = this.GameMemory.ReadFloat(entityEntry + 0x258);
                        EntitySpeed = this.GameMemory.ReadFloat(entityEntry + 0x2C8); //0x1D8
                    }
                    else
                    {
                        // Try Get NPC Position //
                        EntityX = this.GameMemory.ReadFloat(entityEntry + 0x410); //0x3C0
                        if (EntityX == 0)
                        {
                            // Item Position //
                            EntityX = this.GameMemory.ReadFloat(entityEntry + 0x13F0); //0x13E0
                            EntityY = this.GameMemory.ReadFloat(entityEntry + 0x13E4);
                            EntityZ = this.GameMemory.ReadFloat(entityEntry + 0x13E8);
                        }
                        else
                        {
                            // NPC Position //
                            EntityY = this.GameMemory.ReadFloat(entityEntry + 0x414);
                            EntityZ = this.GameMemory.ReadFloat(entityEntry + 0x418);

                            EntitySpeed = this.GameMemory.ReadFloat(entityEntry + 0x2C8); //0x1D8
                            EntityYaw = this.GameMemory.ReadFloat(entityEntry + 0x2E0); //0x1F0
                        }
                    }
		    
		            long entityPositionOffset = this.GameMemory.ReadInt64(entityEntry + 0x198);
                    EntityX = this.GameMemory.ReadFloat(entityPositionOffset + 0x110);
                    EntityY = this.GameMemory.ReadFloat(entityPositionOffset + 0x114);
                    EntityZ = this.GameMemory.ReadFloat(entityPositionOffset + 0x118);
		    
                    // Create New Entity //
                    ENTITY currentEntity = new ENTITY()
                    {
                        Id = EntityId,
                        Type = EntityType,
                        Name = EntityName,
                        Pos = new Vector3(EntityX, EntityY, EntityZ),
                        Distance = Vector3.Distance(new Vector3(EntityX, EntityY, EntityZ), PlayerPosition),
                        Yaw = EntityYaw,
                        Pitch = EntityPitch,
                        Speed = EntitySpeed
                    };

                    // Append New Entity //
                    Entity.Add(currentEntity);

                    switch (EntityType)
                    {
                        case 0x04/*Player*/:
                        case 0x0C/*Zombie*/:
                        case 0x13/*Deer*/:
                        case 0x14/*Wolf*/:
                        case 0x50/*Bear*/:
                        case 0x55/*Rabbit*/:
                        case 0x5b/*Zombie*/:

                            // Aiming //
                           /* Vector3 aimingTo = Vector3.Zero;
                            if (Aimed == true || currentEntity.Distance > 300f)
                            {
                                // Already aimed, not in range, entity is dead
                            }
                            else if (ModifierKeys.HasFlag(Keys.Control) || Convert.ToBoolean(Native.GetAsyncKeyState(Keys.XButton1) & 0x8000) == true)
                            {
                                if (AimedEntity != null && AimedEntity.Id == currentEntity.Id)
                                {
                                    if (Convert.ToBoolean(this.GameMemory.ReadByte(entityEntry + 0x136C)) == false)
                                    {
                                        // Target is dead //
                                        AimedEntity = null;
                                    }
                                    else if (AimedUpdate >= DateTime.Now)
                                    {
                                        Aimed = true;
                                        AimedEntity = currentEntity;
                                        Vector3 AIM; float offsetY = 1.0f;
                                        if (AimedEntity.Type == 0x55) offsetY = 0.20f;
                                        if (AimedEntity.Type == 0x13) offsetY = 0.75f;
                                        if (AimedEntity.Type == 0x14) offsetY = 0.50f;
                                        if (AimedEntity.Type == 0x50) offsetY = 0.65f;
                                        if (WorldToScreen(new Vector3(AimedEntity.Pos.X, AimedEntity.Pos.Y + offsetY, AimedEntity.Pos.Z), out AIM))
                                        {
                                            float moveOffsetX = AIM.X - this.GameWindowCenter.X;
                                            float moveOffsetY = AIM.Y - this.GameWindowCenter.Y;
                                            DrawText("Aimed at " + AimedEntity.Name + ": " + moveOffsetX + ", " + moveOffsetY, ref TextRegion, Color.Red);
                                            Native.mouse_event(0x0001, (short)moveOffsetX, (short)moveOffsetY, 0, 0);
                                            AimedUpdate = DateTime.Now.AddMilliseconds(50);
                                        }
                                    }
                                }
                                else if (AimedEntity == null && WorldToScreen(new Vector3(EntityX, EntityY + 1f, EntityZ), out aimingTo))
                                {
                                    float distance = Vector2.Distance(new Vector2(aimingTo.X, aimingTo.Y), new Vector2(this.GameWindowCenter.X, this.GameWindowCenter.Y));
                                    if (distance < 100f) { AimedEntity = currentEntity; Aimed = true; AimedUpdate = DateTime.Now.AddMilliseconds(50); }
                                }
                            }
                            else
                            {
                                AimedEntity = null;
                            }
                    */

                            // Show Entity when ESP enabled //
                            if (ShowESP) // && this.GameMemory.ReadFloat(entityEntry + 0x1CC) == 1f)
                            {
                                Byte EntityAlive = this.GameMemory.ReadByte(entityEntry + 0x137C); //0x136C
                                if (HideDead == false || Convert.ToBoolean(EntityAlive) == true)
                                {
                                    // Deer or Rabbit //
                                    if (EntityType == 0x13 || EntityType == 0x55)
                                    {
                                        if (ShowAnimals) this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.DarkGreen, true, false, BoxedAnimals, 2f, EntityYaw, EntityPitch);
                                    }
                                    // Player //
                                    else if (EntityType == 0x04)
                                    {
                                            // Show HP
                                        long num22 = this.GameMemory.ReadInt64(entityEntry + 0x4068); //old:  0x4058L   old old 0x3fa8L 0x4068
                                        uint num23 = this.GameMemory.ReadUInt32(num22);
                                        for (uint j = 1; (num23 != 0x30) && (j < 50); j++)
                                        {
                                            num22 = this.GameMemory.ReadInt64(num22 + 0xf8L);
                                            num23 = this.GameMemory.ReadUInt32(num22);
                                        }
                                        uint playerHP = this.GameMemory.ReadUInt32(num22 + 0xb0L) / 100;

                                        int Hue = (int)(120f * (float)playerHP / 100f); // Hue  red at 0� green at 120�
                                        Color color = ColorTranslator.FromWin32(ColorHLSToRGB(Hue, 120, 240)); // H,L,S;
                                        string playerNameHP = EntityName + " " + playerHP.ToString() + "%";
                                        if (ShowPlayers) this.EntityToScreen(new Vector3(EntityX, EntityY + 1f, EntityZ), playerNameHP, color, true, false, BoxedPlayers, 2f, EntityYaw, EntityPitch);
                                        //if (ShowPlayers) this.EntityToScreen(new Vector3(EntityX, EntityY + 1f, EntityZ), EntityName, Color.SkyBlue, true, false, BoxedPlayers, 2f, EntityYaw, EntityPitch);
                                    }
                                    // Aggressive NPC (Wolf, Bear, Zombies) //
                                    else
                                    {
                                        if (ShowAggressive) this.EntityToScreen(new Vector3(EntityX, EntityY + 1f, EntityZ), EntityName, Color.Red, true, false, BoxedAggressive, 2f, EntityYaw, EntityPitch);
                                    }
                                }
                            }
                            break;

                        case 0x2E: // Loot
                            if (ShowESP && ShowItems)
                            {
                                if (EntityName.Contains("First"))
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.Green, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                                else
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.GreenYellow, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                            }
                            break;

                        case 0x1B: // Campfire
                        case 0x6D: // Stash
                        case 0x9C: // Land Mine
                            if (ShowESP && ShowItems) this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.SaddleBrown, true, false, BoxedItems, 1f, 0f, 0f);
                            break;

                        case 0x2F: // Furnace //
                        case 0x33: // Storage Container
                        case 0x35: // Animal Trap
                        case 0x36: // Dew Collector
                        case 0x53: // Barbeque
                            if (ShowESP && ShowContainers) this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.Gray, true, false, BoxedItems, 1f, 0f, 0f);
                            break;

                        case 0x34: // Weapons
                            if (ShowESP && ShowWeapons)
                            {
                                if (EntityName.Contains("Shotgun"))
                                {
this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.OrangeRed, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                                else if (EntityName.Contains("AR15"))
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.OrangeRed, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                                else if (EntityName.Contains("M1911"))
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.OrangeRed, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                                else
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.GreenYellow, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                            }
                            break;

                        case 0x15: // Ammo
                        if (ShowESP && ShowAmmo)
                        {
                            if (EntityName.Contains(".223"))
                            {
                                this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.Orange, true, false, BoxedItems, 1f, 0f, 0f);
                            }
                            else if (EntityName.Contains("Shotgun"))
                            {
                                this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.Orange, true, false, BoxedItems, 1f, 0f, 0f);
                            }
                            else if (EntityName.Contains(".45"))
                            {
                                this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.Orange, true, false, BoxedItems, 1f, 0f, 0f);
                            }
                        }
                        break;

                        case 0x11: // OffRoad
                        case 0x72: // Pickup
                        case 0x76: // PoliceCar
                            if (ShowESP && ShowVehicles) this.EntityToScreen(new Vector3(EntityX, EntityY + 1f, EntityZ), EntityName, Color.HotPink, true, false, BoxedVehicles, 2f, 0f, 0f);
                            break;

                        case 0x2C: // Resources, Battary, Turbo, Sparkplugs
                            if (ShowESP && ShowItems)
                            {
                                if (EntityName.Contains("Battery") || EntityName.Contains("Turbo") || EntityName.Contains("Headlights") || EntityName.Contains("Sparkplugs"))
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.LavenderBlush, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                                // ******ACTUAL LOCATION
                                if (EntityName.Contains("First Aid Kit")) 
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.Lime, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                                else
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.White, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                            }
                            break;

                        case 0x16: // Dresser
                        case 0x17: // Armoire
                        case 0x19: // World Doors
                        case 0x1D: // Cabinets
                        case 0x1E: // Cabinets
                        case 0x21: // Cabinets
                        case 0x22: // Cabinets
                        case 0x23: // Cabinets
                        case 0x25: // Refrigerator
                        case 0x26: // Garbage Can
                        case 0x28: // Cabinets
                        case 0x29: // Desk
                        case 0x27: // Dumpster
                        case 0x30: // File Cabinet
                        case 0x31: // Tool Cabinet
                        case 0x37: // Recycle Bin (with fire)
                        case 0x38: // Punji Sticks
                        case 0x3D: // Wooded Barricade
                        case 0x3E: // Water Well
                        case 0x3F: // Armoire
                        case 0x40: // Dresser
                        case 0x42: // Chest
                        case 0x44: // Wrecked Sedan
                        case 0x45: // Wrecked Van
                        case 0x46: // Wrecked Truck
                        case 0x49: // Ottoman
                        case 0x4A: // Ottoman
                        case 0x4F: // Designer-placed(?) Door
                        case 0x5D: // File Cabinet
                        case 0x61: // Cabinets
                        case 0x63: // Cabinets
                        case 0x6F: // Locker
                        case 0x70: // Weapon Locker
                        case 0x71: // Glass Cabinet
                        case 0x79: // Designer-placed(?) Door
                        case 0x7A: // Resting (Bed)
                        case 0x7B: // Designer-placed(?) Door
                        case 0x7C: // Designer-placed(?) Door
                        case 0x80: // Cabinets
                        case 0x81: // Cabinets
                        case 0x82: // Cabinets
                        case 0x83: // Cabinets
                        case 0x84: // Cabinets
                        case 0x85: // Cabinets
                        case 0x86: // Cabinets
                        case 0x87: // Cabinets
                        case 0x88: // Cabinets
                        case 0xA1: // Washing Machine
                        case 0xA2: // Dryer
                        case 0x7D: // IO.FireHydrant
                        case 0x7E: // IO.FireHydrant
                            //this.WorldToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName + "[" + EntityId.ToString("X2") + "]", Color.Gray, true, false, false, 0f, 0f);
                            break;

                        case 0x4C: // Shed
                        case 0x5F: // Metal Wall/Gate
                        case 0x62: // Basic Shack Door
                        case 0x6E: // Desk Foundation
                        case 0x9E: // Metal Door
                        case 0xA6: // Large Shelter
                        case 0xA7: // Shed
                            //this.WorldToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName + "[" + EntityId.ToString("X2") + "]", Color.Gray, true, false, false, 0f, 0f);
                            break;

                        default: // Other Items
                            if (ShowESP && ShowItems)
                            {
                                if (EntityName.Contains("First"))
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName, Color.Green, true, false, BoxedItems, 1f, 0f, 0f);
                                }
                                else
                                {
                                    this.EntityToScreen(new Vector3(EntityX, EntityY, EntityZ), EntityName + "[" + EntityType.ToString("X2") + "]", Color.White, true, false, false, 1f, 0f, 0f);
                                }
                            }
                            break;
                    }
                    entityEntry = this.GameMemory.ReadInt64(entityEntry + 0x410);
                }

                if (Aimed == false) AimedEntity = null;

                if (ShowEntityLists)
                {
                    ENTITY[] playerList = Entity.Where(E => E.Type == 0x04).OrderBy(E => E.Distance).ToArray();
                    ENTITY[] aggressiveList = Entity.Where(E => E.Type == 0x0C || E.Type == 0x14 || E.Type == 0x50 || E.Type == 0x5b).OrderBy(E => E.Distance).ToArray();
                    ENTITY[] animalsList = Entity.Where(E => E.Type == 0x13 || E.Type == 0x55).OrderBy(E => E.Distance).ToArray();
                    ENTITY[] vehiclesList = Entity.Where(E => E.Type == 0x11 || E.Type == 0x72 || E.Type == 0x76).OrderBy(E => E.Distance).ToArray();

                    int itemY = (aggressiveList.Length > 10 ? 150 : aggressiveList.Length * 15);
                    foreach (ENTITY entity in aggressiveList)
                    {
                        DrawText(entity.Name + " [" + Math.Round(entity.Distance).ToString() + " m]", this.GameWindowRect.Left + 20, this.GameWindowRect.Bottom - itemY - 15, Color.Red);
                        itemY -= 15; if (itemY <= 0) break;
                    }

                    itemY = (animalsList.Length > 10 ? 150 : animalsList.Length * 15);
                    foreach (ENTITY entity in animalsList)
                    {
                        DrawText(entity.Name + " [" + Math.Round(entity.Distance).ToString() + " m]", this.GameWindowRect.Left + 120, this.GameWindowRect.Bottom - itemY - 15, Color.Green);
                        itemY -= 15; if (itemY <= 0) break;
                    }

                    itemY = (vehiclesList.Length > 10 ? 150 : vehiclesList.Length * 15);
                    foreach (ENTITY entity in vehiclesList)
                    {
                        DrawText(entity.Name + " [" + Math.Round(entity.Distance).ToString() + " m]", this.GameWindowRect.Left + 220, this.GameWindowRect.Bottom - itemY - 15, Color.HotPink);
                        itemY -= 15; if (itemY <= 0) break;
                    }

                    itemY = (playerList.Length > 10 ? 150 : playerList.Length * 15);
                    foreach (ENTITY entity in playerList)
                    {
                        DrawText(entity.Name + " [" + Math.Round(entity.Distance).ToString() + " m] " + playerHP.ToString() + "%" , this.GameWindowRect.Left + 320, this.GameWindowRect.Bottom - itemY - 15, Color.SkyBlue);
                        itemY -= 15; if (itemY <= 0) break;
                    }
                }


                if (ShowRadar)
                {
                    DrawFilledBox(this.GameWindowRect.Right - 225, this.GameWindowRect.Top + 50, 201f, 201f, Color.DarkOliveGreen, RadarTransparency);
                    DrawBoxAbs(this.GameWindowRect.Right - 225, this.GameWindowRect.Top + 50, 201f, 201f, 1f, Color.Black);
                    DrawLine(this.GameWindowRect.Right - 125, this.GameWindowRect.Top + 50, this.GameWindowRect.Right - 125, this.GameWindowRect.Top + 251, 1f, Color.Black);
                    DrawLine(this.GameWindowRect.Right - 225, this.GameWindowRect.Top + 150, this.GameWindowRect.Right - 24, this.GameWindowRect.Top + 150, 1f, Color.Black);
                    RadarCenter = new Vector2(this.GameWindowRect.Right - 125, this.GameWindowRect.Top + 125 + 25);
                    DrawFilledBox(RadarCenter.X - 1f, RadarCenter.Y - 1f, 3f, 3f, Color.White);
                    if (RadarCenter.Length() > 0f)
                    {
                        foreach (ENTITY entity in Entity)
                        {
                            Vector2 pointToRotate = new Vector2(entity.Pos.X, entity.Pos.Z);
                            Vector2 vector3 = new Vector2(player_X, player_Z);
                            pointToRotate = vector3 - pointToRotate;
                            float num30 = pointToRotate.Length() * 0.5f;
                            num30 = Math.Min(num30, 90f);
                            pointToRotate.Normalize();
                            pointToRotate = (Vector2)(pointToRotate * num30);
                            pointToRotate += RadarCenter;
                            pointToRotate = RotatePoint(pointToRotate, RadarCenter, player_D, true);
                            if (entity.Type == 0x04 && RadarPlayers)
                            {
                                DrawFilledBox(pointToRotate.X, pointToRotate.Y, 3f, 3f, Color.SkyBlue);
                            }
                            if ((entity.Type == 0x0C || entity.Type == 0x14 || entity.Type == 0x50 || entity.Type == 0x5b) && RadarAggressive)
                            {
                                DrawFilledBox(pointToRotate.X, pointToRotate.Y, 3f, 3f, Color.Red);
                            }
                            if ((entity.Type == 0x13 || entity.Type == 0x55) && RadarAnimals)
                            {
                                DrawFilledBox(pointToRotate.X, pointToRotate.Y, 3f, 3f, Color.LightGreen);
                            }
                            if ((entity.Type == 0x11 || entity.Type == 0x72 || entity.Type == 0x76) && RadarVehicles)
                            {
                                DrawFilledBox(pointToRotate.X, pointToRotate.Y, 3f, 3f, Color.HotPink);
                            }
                        }
                    }
                }

                if (ShowCities)
                {
                    this.EntityToScreen(new Vector3(-129f, 40f, -1146f), "Pleasant Valley", Color.DarkViolet, true, false, false, 1f, 0f, 0f);
                    this.EntityToScreen(new Vector3(-1233f, 90f, 1855f), "Cranberry", Color.DarkViolet, true, false, false, 1f, 0f, 0f);
                    this.EntityToScreen(new Vector3(2003f, 50f, 2221f), "Ranchito", Color.DarkViolet, true, false, false, 1f, 0f, 0f);
                }

                if (Main.ShowMap && (DXTextrureMap != null || DXTextrureMapLarge != null))
                {
                    DXSprite.Begin(SpriteFlags.AlphaBlend);
                    if (Main.ShowMapLarge && DXTextrureMapLarge != null)
                    {
                        map_pos_x = RemapValue(player_X, 4000f, -4000f, -512f, 512f);
                        map_pos_z = RemapValue(player_Z, -4000f, 4000f, -512f, 512f);
                        DXSprite.Draw(DXTextrureMapLarge, new Vector3(512f, 512f, 0f), new Vector3(this.GameWindowCenter.X, this.GameWindowCenter.Y, 0f), Color.FromArgb(MapTransparency, 0xff, 0xff, 0xff));
                    }
                    else if (DXTextrureMap != null)
                    {
                        map_pos_x = RemapValue(player_X, 4000f, -4000f, -265f, 265f);
                        map_pos_z = RemapValue(player_Z, -4000f, 4000f, -265f, 265f);
                        DXSprite.Draw(DXTextrureMap, new Vector3(256f, 256f, 0f), new Vector3(this.GameWindowCenter.X, this.GameWindowCenter.Y, 0f), Color.FromArgb(MapTransparency, 0xff, 0xff, 0xff));
                    }
                    DXSprite.End();
                    float direction = Main.player_D * -1f;
                    float fromX = (float)((this.GameWindowCenter.X + map_pos_z) + (20.0 * Math.Cos(direction)));
                    float fromY = (float)((this.GameWindowCenter.Y + map_pos_x) + (20.0 * Math.Sin(direction)));
                    DrawFilledBox((this.GameWindowCenter.X + map_pos_z) - 2f, (this.GameWindowCenter.Y + map_pos_x) - 2f, 6f, 6f, Color.Magenta);
                    DrawLine(fromX, fromY, (this.GameWindowCenter.X + map_pos_z) + 1f, (this.GameWindowCenter.Y + map_pos_x) + 1f, 1f, Color.PaleVioletRed);
                }

                DXDevice.EndScene();
                DXDevice.Present();
                Thread.Sleep(1);
            }
            DXDevice.Dispose();
            Application.Exit();
        }

        public void DoAiming()
        {
            while (true)
            {
                if (Aimed == true && AimedEntity != null)
                {

                }
                Thread.Sleep(1);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x312)
            {
                switch ((int)m.WParam)
                {
                    case 0:
                        if (this.Settings != null && this.Settings.Visible)
                        {
                            this.Settings.Close();
                            break;
                        }
                        this.Settings = new Settings(); this.Settings.Show();
                        this.Settings.Location = new Point(this.GameWindowRect.Right - this.Settings.Width - 20, this.GameWindowRect.Top + 200);
                        Native.SetForegroundWindow(this.GameMemory.Process.MainWindowHandle);
                        break;

                    case 1:
                        ShowRadar = !ShowRadar;
                        Main.Ini.IniWriteValue("Radar", "Show", ShowRadar.ToString());
                        break;

                    case 2:
                        ShowMap = !ShowMap;
                        break;

                    case 3:
                        ShowMapLarge = !ShowMapLarge;
                        Main.Ini.IniWriteValue("Map", "LargeMap", Main.ShowMapLarge.ToString());
                        break;

                    case 4:
                        Main.IsRunning = false;
                        break;
                }
            }
            base.WndProc(ref m);
        }

        public static void MoveMouse(int xDelta, int yDelta)
        {
            Native.mouse_event(1, xDelta, yDelta, 0u, 0u);
        }

        public static void MoveMouseTo(int x, int y)
        {
            Native.mouse_event(0x8000, x, y, 0, 0);
            Native.mouse_event(1, x, y, 0, 0);
        }
    }
}