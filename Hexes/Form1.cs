using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hexes
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        const float pi = (float)Math.PI;
        public static void DrawHex(Graphics G, float x, float y, float R, Brush brush)
        {
            PointF[] point = new PointF[6];
            float angle = pi / 6;
            for (int i = 0; i < 6; i++)
            {

                float xx = x + R * (float)Math.Cos(angle), yy = y + R * (float)Math.Sin(angle);
                point[i] = new PointF(xx, yy);
                angle += pi / 3;
            }
            G.FillPolygon(brush, point);
            G.DrawPolygon(Pens.Black, point);
        }
        abstract class Hex
        {
            public abstract void Draw(Graphics G, float R);
            public abstract void Draw(Graphics G,Brush brush, float R);
            public abstract void DrawTransparentHexColor(Graphics G, HexColor player_color, float R);
            public abstract float X { get; }
            public abstract float Y { get; }
            public abstract int GameX { get; }
            public abstract int GameY { get; }
            public abstract void Repaint(HexColor color);
            public abstract void TryRepaint(Hex[,] hexes,Player[] player, ref Player ActivePlayer);
            public abstract void FindBridge(Hex[,] hex, ref bool[,] visited, HexColor color, ref bool LeftEnd, ref bool RightEnd);
            public abstract bool IsBridgeBuilt(Hex[,] hex, HexColor color_player);
            public abstract void FindFreeHexRec(Hex[,] hexes, ref bool[,] visited,ref bool hex_chosen,ref Hex ChosenHex);
            public abstract HexColor Hex_color { get; }
        }
        abstract class ColoredHex : Hex
        {
            protected HexColor color;
            public ColoredHex(HexColor color)
            {
                this.color = color;
            }
            public override void Repaint(HexColor color)
            {
                this.color = color;
            }
            public override HexColor Hex_color => color;
        }
        class RealHex : ColoredHex
        {
            int game_x, game_y;
            float x, y;
            public RealHex(int game_x, int game_y, float x, float y):base(white_hex_color)
            {
                this.x = x; this.y = y;
                this.game_x = game_x; this.game_y = game_y;
            }
            public override void Draw(Graphics G, float R)
            {
                DrawHex(G, x, y, R, color.PaintColor);
            }
            public override void Draw(Graphics G,Brush brush, float R)
            {
                DrawHex(G, x, y, R, brush);
            }
            public override void DrawTransparentHexColor(Graphics G,HexColor player_color, float R)
            {
                color.TryDrawTransparentColorHex(G, this, player_color, R);
            }
            public override float X => x;
            public override float Y => y;
            public override int GameX => game_x;
            public override int GameY => game_y;
            public override void TryRepaint(Hex[,] hexes, Player[] player, ref Player ActivePlayer)
            {
                color.TryChangeColorHex(this,hexes, player, ref ActivePlayer);
            }
            public override bool IsBridgeBuilt(Hex[,] hex, HexColor color_player)
            {
                bool LeftEnd = false, RightEnd = false;
                bool[,] visited = new bool[11, 11];
                FindBridge(hex, ref visited, color_player, ref LeftEnd, ref RightEnd);
                return LeftEnd && RightEnd;
            }
            public override void FindBridge(Hex[,] hex, ref bool[,] visited, HexColor color_player, ref bool LeftEnd, ref bool RightEnd)
            {
                color_player.FindBridgeRec(this,hex,ref visited,color_player,ref LeftEnd,ref RightEnd);
            }
            public override void FindFreeHexRec(Hex[,] hexes, ref bool[,] visited, ref bool hex_chosen, ref Hex ChosenHex)
            {
                if(!hex_chosen && !visited[game_x, game_y])
                {
                    color.TryChoseFreeHex(this, ref hex_chosen, ref ChosenHex);
                    visited[game_x, game_y] = true;
                    for(int i=0;i<6 && !hex_chosen; i++)
                    {
                        int next_x = (12 + dir[i, 0] + game_x) % 12, next_y = (12 + dir[i, 1] + game_y) % 12;
                        hexes[next_x, next_y].FindFreeHexRec(hexes, ref visited, ref hex_chosen, ref ChosenHex);
                    }
                }
            }
        }
        class EndHex:ColoredHex
        {
            int id;
            public EndHex(HexColor color, int id):base(color)
            {
                this.id = id;
            }
            public override void Draw(Graphics G, float R) { }
            public override void Draw(Graphics G, Brush brush, float R) { }
            public override void DrawTransparentHexColor(Graphics G, HexColor player_color, float R) { }
            public override float X => -1;
            public override float Y => -1;
            public override int GameX => -1;
            public override int GameY => -1;
            public override void Repaint(HexColor color) { }
            public override void TryRepaint(Hex[,] hexes, Player[] player, ref Player ActivePlayer) { }
            public override void FindBridge(Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd)
            {
                color.FindEndBridge(player_color, ref LeftEnd, ref RightEnd,id);
            }
            public override bool IsBridgeBuilt(Hex[,] hex, HexColor color_player) => false;
            public override void FindFreeHexRec(Hex[,] hexes, ref bool[,] visited, ref bool hex_chosen, ref Hex ChosenHex) { }
        }
        class NullHex : Hex
        {
            public override void Draw(Graphics G, float R) { }
            public override void Draw(Graphics G, Brush brush, float R) { }
            public override void DrawTransparentHexColor(Graphics G, HexColor player_color, float R) { }
            public override float X=>-1;
            public override float Y=>-1;
            public override int GameX => -1;
            public override int GameY => -1;
            public override void Repaint(HexColor color) { }
            public override void TryRepaint(Hex[,] hexes, Player[] player, ref Player ActivePlayer) { }
            public override void FindBridge(Hex[,] hex, ref bool[,] visited, HexColor color, ref bool LeftEnd, ref bool RightEnd) { }
            public override bool IsBridgeBuilt(Hex[,] hex, HexColor color_player) => false;
            public override void FindFreeHexRec(Hex[,] hexes, ref bool[,] visited, ref bool hex_chosen, ref Hex ChosenHex) { }
            public override HexColor Hex_color =>white_hex_color;
        }
        static Hex null_hex = new NullHex();
        abstract class HexColor
        {
            protected Brush brush,brush_transparent;
            public HexColor(Brush brush, Brush brush_transparent)
            {
                this.brush = brush; this.brush_transparent = brush_transparent;
            }
            public Brush PaintColor => brush;
            public Brush PaintTransparentColor => brush_transparent;
            public abstract void TryChangeColorHex(Hex hex,Hex[,] hexes,Player[] player, ref Player ActivePlayer);
            public abstract void TryDrawTransparentColorHex(Graphics G, Hex hex, HexColor player_color, float R);
            public abstract void FindBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor color,ref bool LeftEnd,ref bool RightEnd);
            public abstract void FindRedBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd);
            public abstract void FindBlueBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd);
            public abstract void FindEndBridge(HexColor player_color, ref bool LeftEnd, ref bool RightEnd,int id);
            public abstract void FindEndBlueBridge(ref bool LeftEnd, ref bool RightEnd, int id);
            public abstract void FindEndRedBridge(ref bool LeftEnd, ref bool RightEnd, int id);
            public abstract void TryChoseFreeHex(Hex current_hex, ref bool hex_chosen, ref Hex ChosenHex);
            public abstract void FindRedBridge(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd);
            public abstract void FindBlueBridge(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd);
        }
        class HexRedColor : HexColor
        {
            public HexRedColor() : base(Brushes.IndianRed,new SolidBrush(Color.FromArgb(100,Color.Red)))
            {
                
            }
            public override void TryChangeColorHex(Hex hex, Hex[,] hexes, Player[] player, ref Player ActivePlayer) 
            {

            }
            public override void TryDrawTransparentColorHex(Graphics G,Hex hex, HexColor player_color, float R) { }
            public override void FindBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd)
            {
                FindRedBridgeRec(current_hex, hex, ref visited, player_color, ref LeftEnd, ref RightEnd);
            }
            public override void FindRedBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd)
            {
                int x = current_hex.GameX, y = current_hex.GameY;
                if (!visited[x, y])
                {
                    visited[x, y] = true;
                    for (int i = 0; i < 6; i++)
                    {
                        int dir_x = dir[i, 0], dir_y = dir[i, 1];
                        int next_x = (x + dir_x + 13) % 13, next_y = (y + dir_y + 13) % 13;
                        Hex next_hex = hex[next_x, next_y];HexColor color_next = next_hex.Hex_color;
                        color_next.FindRedBridge(next_hex, hex, ref visited, player_color, ref LeftEnd, ref RightEnd);
                        //next_hex.FindBridge(hex, ref visited, player_color, ref LeftEnd, ref RightEnd);
                    }
                }
            }
            public override void FindRedBridge(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd)
            {
                current_hex.FindBridge(hex, ref visited, player_color, ref LeftEnd, ref RightEnd);
            }
            public override void FindBlueBridge(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd)
            {

            }
            public override void FindBlueBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd)
            {

            }
            public override void FindEndBridge(HexColor player_color, ref bool LeftEnd, ref bool RightEnd, int id)
            {
                player_color.FindEndRedBridge(ref LeftEnd, ref RightEnd, id);
            }
            public override void FindEndBlueBridge(ref bool LeftEnd, ref bool RightEnd, int id) 
            {

            }
            public override void FindEndRedBridge(ref bool LeftEnd, ref bool RightEnd, int id)
            {
                LeftEnd = id == 0|| LeftEnd; RightEnd = id == 1|| RightEnd;
            }
            public override void TryChoseFreeHex(Hex current_hex, ref bool hex_chosen, ref Hex ChosenHex)
            {

            }
        }
        class HexBlueColor : HexColor
        {
            public HexBlueColor() : base(Brushes.RoyalBlue, new SolidBrush(Color.FromArgb(100, Color.Blue)))
            {
            }
            public override void TryChangeColorHex(Hex hex, Hex[,] hexes, Player[] player, ref Player ActivePlayer) 
            {
  
            }
            public override void TryDrawTransparentColorHex(Graphics G, Hex hex, HexColor player_color, float R) { }
            public override void FindBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor color, ref bool LeftEnd, ref bool RightEnd)
            {
                FindBlueBridgeRec(current_hex, hex, ref visited, color,ref LeftEnd,ref RightEnd);
            }
            public override void FindRedBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor color, ref bool LeftEnd, ref bool RightEnd)
            {

            }

            public override void FindBlueBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd)
            {
                int x = current_hex.GameX, y = current_hex.GameY;
                if (!visited[x, y])
                {
                    visited[x, y] = true;
                    for (int i = 0; i < 6; i++)
                    {
                        int dir_x = dir[i, 0], dir_y = dir[i, 1];
                        int next_x = (x + dir_x + 13) % 13, next_y = (y + dir_y + 13) % 13;
                        Hex next_hex = hex[next_x, next_y]; HexColor color_next = next_hex.Hex_color;
                        color_next.FindBlueBridge(next_hex, hex, ref visited, player_color, ref LeftEnd, ref RightEnd);
                        //next_hex.FindBridge(hex, ref visited, color,ref LeftEnd,ref RightEnd);
                    }
                }
            }
            public override void FindRedBridge(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd)
            {
                
            }
            public override void FindBlueBridge(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd)
            {
                current_hex.FindBridge(hex, ref visited, player_color, ref LeftEnd, ref RightEnd);
            }
            public override void FindEndBridge(HexColor player_color, ref bool LeftEnd, ref bool RightEnd, int id)
            {
                player_color.FindEndBlueBridge(ref LeftEnd, ref RightEnd, id);
            }
            public override void FindEndBlueBridge(ref bool LeftEnd, ref bool RightEnd, int id)
            {
                LeftEnd = id == 0 || LeftEnd; RightEnd = id == 1 || RightEnd;
            }
            public override void FindEndRedBridge(ref bool LeftEnd, ref bool RightEnd, int id)
            {
                
            }
            public override void TryChoseFreeHex(Hex current_hex, ref bool hex_chosen, ref Hex ChosenHex)
            {

            }
        }
        class HexWhiteColor : HexColor
        {
            public HexWhiteColor() : base(Brushes.White, Brushes.Transparent)
            {

            }
            public override void TryChangeColorHex(Hex hex, Hex[,] hexes, Player[] player, ref Player ActivePlayer)
            {
                hex.Repaint(ActivePlayer.Hex_color);
                bool player_win = hex.IsBridgeBuilt(hexes, ActivePlayer.Hex_color);
                if (!player_win)
                    ActivePlayer = player[(ActivePlayer.ID + 1) % 2];
                else
                {
                    null_player.GameInfo = ActivePlayer.Name + " выиграли!!!";
                    ActivePlayer =null_player;
                }
            }
            public override void TryDrawTransparentColorHex(Graphics G, Hex hex, HexColor player_color,float R)
            {
                hex.Draw(G, player_color.PaintTransparentColor, R);
            }
            public override void FindBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor color, ref bool LeftEnd, ref bool RightEnd) { }
            public override void FindRedBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor color, ref bool LeftEnd, ref bool RightEnd) { }
            public override void FindBlueBridgeRec(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor color, ref bool LeftEnd, ref bool RightEnd) { }
            public override void FindEndBridge(HexColor color, ref bool LeftEnd, ref bool RightEnd, int id) { }
            public override void FindEndBlueBridge(ref bool LeftEnd, ref bool RightEnd, int id) { }
            public override void FindEndRedBridge(ref bool LeftEnd, ref bool RightEnd, int id) { }
            public override void TryChoseFreeHex(Hex current_hex,ref bool hex_chosen, ref Hex ChosenHex)
            {
                ChosenHex = current_hex; hex_chosen = true;
            }
            public override void FindRedBridge(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd) { }
            public override void FindBlueBridge(Hex current_hex, Hex[,] hex, ref bool[,] visited, HexColor player_color, ref bool LeftEnd, ref bool RightEnd) { }

        }
        static HexColor white_hex_color = new HexWhiteColor(), red_hex_color = new HexRedColor(), blue_hex_color = new HexBlueColor();
        abstract class Player
        {
            public abstract string Name { get; }
            public abstract string GameInfo { get; set; }
            public abstract int ID { get; }
            public abstract HexColor Hex_color { get; }
            public abstract void TrySetColorOnHex(Player[] Player, ref Player ActivePlayer);
            public abstract Hex PlayerMoveMouse(Cell[,] cell, float x, float y, int cell_x, int cell_y, float width_cell, float height_cell);
            public abstract void DoTurn(Hex[,] hex, Player[] player, ref Player ActivePlayer, Graphics G, float R);
        }
        abstract class RealPlayer : Player
        {
            string name;
            protected int id;
            protected HexColor hex_color;
            public RealPlayer(int id,string name, HexColor hex_color)
            {
                this.id = id;this.name = name;this.hex_color = hex_color;
            }
            public override string Name => name;
            public override int ID => id;
            public override HexColor Hex_color => hex_color;
            public override string GameInfo 
            {
                get => "Ходят " + name;
                set
                {

                }
            }
        }
        class HumanPlayer : RealPlayer
        {
            public HumanPlayer(int id, string name, HexColor hex_color) : base(id, name,hex_color) { }
            public override void TrySetColorOnHex(Player[] Player, ref Player ActivePlayer)
            {

            }
            public override Hex PlayerMoveMouse(Cell[,] cell, float x, float y, int cell_x, int cell_y,float width_cell, float height_cell)
            {
                return cell[cell_x, cell_y].GetHex(x, y, width_cell, height_cell);
            }
            public override void DoTurn(Hex[,] hex, Player[] player, ref Player ActivePlayer, Graphics G, float R)
            {

            }
        }
        static Random rnd = new Random();
        class AIPlayer : RealPlayer
        {
            public AIPlayer(int id, string name, HexColor hex_color) : base(id, name,hex_color) { }
            public override void TrySetColorOnHex(Player[] Player, ref Player ActivePlayer)
            {

            }
            public override Hex PlayerMoveMouse(Cell[,] cell, float x, float y, int cell_x, int cell_y, float width_cell, float height_cell)
            {
                return null_hex;
            }
            public override void DoTurn(Hex[,] hex, Player[] player, ref Player ActivePlayer, Graphics G,float R)
            {
                Hex chosen_hex = FindFreeHex(hex);
                chosen_hex.Repaint(hex_color);
                chosen_hex.Draw(G, R);
                ActivePlayer = player[(id + 1) % 2];
            }
            Hex FindFreeHex(Hex[,] hex)
            {
                int x = rnd.Next(0, 11), y = rnd.Next(0, 11);
                bool[,] visited = new bool[11, 11];bool hex_chosen = false;Hex chosen_hex = null_hex;
                hex[x, y].FindFreeHexRec(hex,ref visited,ref hex_chosen, ref chosen_hex);
                return chosen_hex;
            }
        }
        class NullPlayer : Player
        {
            string info = "";
            public override string Name => "";
            public override int ID => -1;
            public override HexColor Hex_color => null;
            public override void TrySetColorOnHex(Player[] Player, ref Player ActivePlayer)
            {

            }
            public override string GameInfo
            {
                get => info;
                set
                {
                    info=value;
                }
            }
            public override Hex PlayerMoveMouse(Cell[,] cell, float x, float y, int cell_x, int cell_y, float width_cell, float height_cell)
            {
                return null_hex;
            }
            public override void DoTurn(Hex[,] hex, Player[] player, ref Player ActivePlayer, Graphics G, float R)
            {

            }
        }
        abstract class Cell
        {
            public abstract Hex GetHex(float x,float y,float width,float height);
        }
        abstract class RealCell :Cell
        {
            protected float x, y;
            public RealCell(float x, float y)
            {
                this.x = x;this.y = y;
            }
            
        }
        class SolidCell : RealCell
        {
            protected Hex hex;
            public SolidCell(float x, float y,Hex hex):base(x,y)
            {
                this.hex = hex;
            }
            public override Hex GetHex(float x, float y, float width, float height)
            {
                return hex;
            }
        }
        abstract class DiagCell : RealCell
        {
            protected Hex[] hex;
            public DiagCell(float x, float y, Hex hex1, Hex hex2) : base(x, y)
            {
                hex = new Hex[] { hex1, hex2 };
            }
        }
        class SecondaryDiagCell : DiagCell
        {
            public SecondaryDiagCell(float x, float y, Hex hex1, Hex hex2) : base(x, y,hex1,hex2)
            {

            }
            public override Hex GetHex(float xx, float yy, float width, float height)
            {
                float x1 = x + width, y1 = y;
                float x2 = x, y2 = y+height;
                float dx = x2 - x1, dy = y2 - y1;
                int p = (dy * (xx - x1) - dx * (yy - y1) > 0).GetHashCode();
                return hex[p];
            }
        }
        class MainDiagCell : DiagCell
        {
            public MainDiagCell(float x, float y, Hex hex1, Hex hex2) : base(x, y, hex1, hex2)
            {

            }
            public override Hex GetHex(float xx, float yy, float width, float height)
            {
                float x1 = x, y1 = y;
                float x2 = x+width, y2 = y + height;
                float dx = x2 - x1, dy = y2 - y1;
                int p = (dy * (xx - x1) - dx * (yy - y1) < 0).GetHashCode();
                return hex[p];
            }
        }
        class NullCell:Cell
        {
            public override Hex GetHex(float xx, float yy, float width, float height) { return null_hex; }
        }
        static Cell null_cell = new NullCell();
        static Player null_player = new NullPlayer();
        static int[,] dir = { { 0, -1 }, { 1, -1 }, { 1, 0 }, { 0, 1 }, { -1, 1},{ -1,0} };
        class Board
        {
            Hex[,] hex = new Hex[13, 13]; float R = 0,xx0,yy0;
            Cell[,] cell = new Cell[24,67];
            PictureBox pic;
            Graphics G;
            float width_cell, height_cell;
            Hex Current_hex = null_hex;
            Label info;
            Player ActivePlayer=null_player, human_blue=new HumanPlayer(1,"Синий",blue_hex_color), human_red = new HumanPlayer(0, "Красные",red_hex_color);
            Player ai_blue = new AIPlayer(1, "Синий",blue_hex_color), ai_red = new HumanPlayer(0, "Красные",red_hex_color);
            Player[] player;
            
            //Player[] player = new Player[] {new H };
            public Board(PictureBox pic,Label info, float R)
            {
                player = new Player[] { human_red, human_blue };
                for (int i = 0; i < 24; i++)
                    for (int j = 0; j < 67; j++)
                        cell[i, j] = null_cell;
                this.pic = pic; this.R = R;
                this.info = info;
                pic.BackgroundImage = new Bitmap(pic.Width, pic.Height);
                G = Graphics.FromImage(pic.BackgroundImage);
                float r = R * (float)Math.Cos(pi / 6); width_cell = r; height_cell = R / 2;
                float x0 = pic.Width / 2, y0 = R + 1;
                xx0 = x0 - 11 * width_cell; yy0 = 1;
                float rr_y = 0;
                for (int i = 0; i < 11; i++)
                {
                    float x = x0 + rr_y * (float)Math.Cos(pi / 3);
                    float y = y0 + rr_y * (float)Math.Sin(pi / 3);
                    float rr_x = 0;
                    for (int j = 0; j < 11; j++)
                    {
                        float xx = x + rr_x * (float)Math.Cos(2 * pi / 3);
                        float yy = y + rr_x * (float)Math.Sin(2 * pi / 3);
                        hex[i, j] = new RealHex(i, j, xx, yy);
                        rr_x += 2 * r;
                        int cell1_x =(int)(((xx- width_cell) -xx0+1)/width_cell), cell1_y = (int)(((yy - height_cell+1) - yy0) / height_cell);
                        cell[cell1_x, cell1_y] =new SolidCell(xx-width_cell,yy-height_cell,hex[i, j]);
                        cell[cell1_x+1, cell1_y] = new SolidCell(xx, yy - height_cell, hex[i, j]);
                        cell[cell1_x, cell1_y+1] = new SolidCell(xx - width_cell, yy, hex[i, j]);
                        cell[cell1_x+1, cell1_y + 1] = new SolidCell(xx, yy, hex[i, j]);
                        int y_up = (67 + (cell1_y - 2)) % 67;
                        Hex h_left = cell[cell1_x, y_up].GetHex(0, 0, width_cell, height_cell);
                        Hex h_right = cell[cell1_x+1, y_up].GetHex(0, 0, width_cell, height_cell);
                        cell[cell1_x, cell1_y - 1] = new SecondaryDiagCell(xx - width_cell, yy - 2 * height_cell, h_left, hex[i, j]);
                        cell[cell1_x+1, cell1_y - 1] = new MainDiagCell(xx, yy - 2 * height_cell, h_right, hex[i, j]);
                    }
                    float xx_ = x + rr_x * (float)Math.Cos(2 * pi / 3);
                    float yy_ = y + rr_x * (float)Math.Sin(2 * pi / 3);
                    int cell_x = (int)((xx_ - xx0 + 1) / width_cell), cell_y = (int)((yy_ - 2* height_cell - yy0 + 1) / height_cell);
                    cell[cell_x, cell_y]= new MainDiagCell(xx_, yy_ - 2 * height_cell, hex[i, 10], null_hex);
                    rr_y += 2 * r;
                }
                cell[11, 63] = new SecondaryDiagCell(11*width_cell+xx0, 63*height_cell+yy0, hex[10, 10], null_hex);
                Hex red_end_hex_left = new EndHex(red_hex_color, 0), red_end_hex_right = new EndHex(red_hex_color, 1);
                Hex blue_end_hex_left = new EndHex(blue_hex_color, 0), blue_end_hex_right = new EndHex(blue_hex_color, 1);
                for (int i = 0; i < 13; i++)
                {
                    hex[11, i] = blue_end_hex_right;
                    hex[12, i] = blue_end_hex_left;
                    hex[i, 11] = red_end_hex_right;
                    hex[i, 12] = red_end_hex_left;
                }
                pic.Paint += Paint;
                pic.MouseMove += MouseMove;
                pic.MouseDown += MouseDown;
                Draw();
                ActivePlayer = player[0];
            }
            public void NewGame()
            {
                ActivePlayer = player[0];
                for (int i = 0; i < 11; i++)
                    for (int j = 0; j < 11; j++)
                        hex[i, j].Repaint(white_hex_color);
                Draw();
                info.Text = ActivePlayer.GameInfo;
                ActivePlayer.DoTurn(hex, player, ref ActivePlayer, G, R);
            }
            public void Draw()
            {
                float x0 = hex[0, 0].X, y0 = hex[0, 0].Y;
                float O_x = hex[5, 5].X, O_y = hex[5, 5].Y;

                float r = R * (float)Math.Cos(pi / 6);
                float B_x = x0, B_y = y0 - 2.5f * R;
                float OB_x = B_x - O_x, OB_y = B_y - O_y;
                float AB_x = (float)(2*(OB_x *Math.Cos(5 * pi / 6) + OB_y * Math.Sin(5 * pi / 6))/ Math.Sqrt(3));
                float AB_y = (float)(2*(-OB_x * Math.Sin(5 * pi / 6) + OB_y * Math.Cos(5 * pi / 6))/ Math.Sqrt(3));
                float A_x = B_x+ AB_x, A_y = B_y + AB_y;
                G.FillPolygon(Brushes.SkyBlue, new PointF[] { new PointF(B_x, B_y), new PointF(O_x,O_y), new PointF(A_x, A_y) });
                float AC_x = (float)(2 * (OB_x * Math.Cos(-5 * pi / 6) + OB_y * Math.Sin(-5 * pi / 6)) / Math.Sqrt(3));
                float AC_y = (float)(2 * (-OB_x * Math.Sin(-5 * pi / 6) + OB_y * Math.Cos(-5 * pi / 6)) / Math.Sqrt(3));
                float C_x = B_x + AC_x, C_y = B_y + AC_y;
                G.FillPolygon(Brushes.OrangeRed, new PointF[] { new PointF(B_x, B_y), new PointF(O_x, O_y), new PointF(C_x, C_y) });
                float D_x = hex[10, 10].X, D_y = hex[10, 10].Y+ 2.5f * R;
                G.FillPolygon(Brushes.OrangeRed, new PointF[] { new PointF(A_x, A_y), new PointF(O_x, O_y), new PointF(D_x, D_y) });
                G.FillPolygon(Brushes.SkyBlue, new PointF[] { new PointF(C_x, C_y), new PointF(O_x, O_y), new PointF(D_x, D_y) });
                foreach (Hex item in hex)
                    item.Draw(G, R);
                pic.Invalidate();
            }
            private void Paint(object sender, PaintEventArgs e)
            {
                Current_hex.DrawTransparentHexColor(e.Graphics, ActivePlayer.Hex_color, R);
            }
            private void MouseMove(object sender, MouseEventArgs e)
            {
                float x = e.X, y = e.Y;
                int cell_x = (int)((x - xx0) / width_cell), cell_y = (int)((y - yy0) / height_cell);
                //Current_hex = cell[cell_x, cell_y].GetHex(x,y, width_cell, height_cell);
                Current_hex = ActivePlayer.PlayerMoveMouse(cell, x, y, cell_x, cell_y, width_cell, height_cell);
                pic.Invalidate();
            }
            private void MouseDown(object sender, MouseEventArgs e)
            {
                Current_hex.TryRepaint(hex,player, ref ActivePlayer);
                Current_hex.Draw(G, R);
                info.Text = ActivePlayer.GameInfo;
                ActivePlayer.DoTurn(hex, player, ref ActivePlayer, G, R);
            }
            public void GameWithAI()
            {
                player[1] = ai_blue;
                ActivePlayer = player[ActivePlayer.ID];
                ActivePlayer.DoTurn(hex, player, ref ActivePlayer, G, R);
            }
            public void GameWithHuman()
            {
                player[1] = human_blue;
            }
        }

        Board board;
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void новаяИграToolStripMenuItem_Click(object sender, EventArgs e)
        {
            board.NewGame();
        }

        private void игратьСИИToolStripMenuItem_Click(object sender, EventArgs e)
        {
            игратьСИИToolStripMenuItem.Checked = !игратьСИИToolStripMenuItem.Checked;
            if (игратьСИИToolStripMenuItem.Checked)
                board.GameWithAI();
            else
                board.GameWithHuman();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            board = new Board(pictureBox1,label1, 15);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}