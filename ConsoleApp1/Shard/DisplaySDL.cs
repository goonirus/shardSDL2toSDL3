/*
*
*   This is the implementation of the Simple Directmedia Layer through C#.   This isn't a course on 
*       graphics, so we're not going to roll our own implementation.   If you wanted to replace it with 
*       something using OpenGL, that'd be a pretty good extension to the base Shard engine.
*       
*   Note that it extends from DisplayText, which also uses SDL.  
*   
*   @author Michael Heron
*   @version 1.0
*     
*   Contributions to the code made by others:
*   @author Aristotelis Anthopoulos (see Changelog for 1.3.0)  
*/

using SDL;
using static SDL.SDL3;
using static SDL.SDL3_image;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Shard
{

    class Line
    {
        private int sx, sy;
        private int ex, ey;
        private int r, g, b, a;

        public int Sx { get => sx; set => sx = value; }
        public int Sy { get => sy; set => sy = value; }
        public int Ex { get => ex; set => ex = value; }
        public int Ey { get => ey; set => ey = value; }
        public int R { get => r; set => r = value; }
        public int G { get => g; set => g = value; }
        public int B { get => b; set => b = value; }
        public int A { get => a; set => a = value; }
    }

    class Circle
    {
        int x, y, rad;
        private int r, g, b, a;

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public int Radius { get => rad; set => rad = value; }
        public int R { get => r; set => r = value; }
        public int G { get => g; set => g = value; }
        public int B { get => b; set => b = value; }
        public int A { get => a; set => a = value; }
    }


    class DisplaySDL : DisplayText
    {
        private List<Transform> _toDraw;
        private List<Line> _linesToDraw;
        private List<Circle> _circlesToDraw;
        private Dictionary<string, IntPtr> spriteBuffer;
        public override void initialize()
        {
            spriteBuffer = new Dictionary<string, IntPtr>();

            base.initialize();

            _toDraw = new List<Transform>();
            _linesToDraw = new List<Line>();
            _circlesToDraw = new List<Circle>();


        }

        public IntPtr loadTexture(Transform trans)
        {
            IntPtr ret = loadTexture(trans.SpritePath);
            if (ret == IntPtr.Zero) return IntPtr.Zero;

            float w = 0;
            float h = 0;

            unsafe
            {
                SDL_Texture* tex = (SDL_Texture*)ret;
                SDL_GetTextureSize(tex, &w, &h);
            }

            trans.Wid = (int)w;
            trans.Ht = (int)h;
            trans.recalculateCentre();

            return ret;
        }

        public IntPtr loadTexture(string path)
        {
            if (spriteBuffer.ContainsKey(path))
                return spriteBuffer[path];

            Debug.getInstance().log("IMG_LoadTexture: " + SDL_GetError());

            unsafe
            {
                SDL_Renderer* renderer = (SDL_Renderer*)_rend;

                byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(path + "\0");
                fixed (byte* pPath = utf8)
                {
                    SDL_Texture* tex = IMG_LoadTexture(renderer, pPath);
                    spriteBuffer[path] = (nint)tex;

                    SDL_SetTextureBlendMode(tex, SDL_BlendMode.SDL_BLENDMODE_BLEND);
                }
            }

            return spriteBuffer[path];
        }

        public override void addToDraw(GameObject gob)
        {
            _toDraw.Add(gob.Transform);

            if (gob.Transform.SpritePath == null)
            {
                return;
            }

            loadTexture(gob.Transform.SpritePath);
        }

        public override void removeToDraw(GameObject gob)
        {
            _toDraw.Remove(gob.Transform);
        }


        void renderCircle(int centreX, int centreY, int rad)
        {
            int dia = (rad * 2);
            byte r, g, b, a;
            int x = (rad - 1);
            int y = 0;
            int tx = 1;
            int ty = 1;
            int error = (tx - dia);

            unsafe
            {
                SDL_Renderer* renderer = (SDL_Renderer*)_rend;
                byte rr = 0, gg = 0, bb = 0, aa = 0;

                SDL_GetRenderDrawColor(renderer, &rr, &gg, &bb, &aa);

                r = rr; g = gg; b = bb; a = aa;
            }

            var points = new List<SDL_Point>();

            // We draw an octagon around the point, and then turn it a bit.  Do 
            // that until we have an outline circle.  If you want a filled one, 
            // do the same thing with an ever decreasing radius.
            while (x >= y)
            {
                points.Add(new SDL_Point { x = centreX + x, y = centreY - y });
                points.Add(new SDL_Point { x = centreX + x, y = centreY + y });
                points.Add(new SDL_Point { x = centreX - x, y = centreY - y });
                points.Add(new SDL_Point { x = centreX - x, y = centreY + y });
                points.Add(new SDL_Point { x = centreX + y, y = centreY - x });
                points.Add(new SDL_Point { x = centreX + y, y = centreY + x });
                points.Add(new SDL_Point { x = centreX - y, y = centreY - x });
                points.Add(new SDL_Point { x = centreX - y, y = centreY + x });

                if (error <= 0)
                {
                    y += 1;
                    error += ty;
                    ty += 2;
                }

                if (error > 0)
                {
                    x -= 1;
                    tx += 2;
                    error += (tx - dia);
                }

                unsafe
                {
                    SDL_Renderer* renderer = (SDL_Renderer*)_rend;
                    foreach (var p in points)
                    {
                        SDL_RenderPoint(renderer, p.x, p.y);
                    }
                }

                points.Clear();
            }

        }

        public override void drawCircle(int x, int y, int rad, int r, int g, int b, int a)
        {
            Circle c = new Circle();

            c.X = x;
            c.Y = y;
            c.Radius = rad;

            c.R = r;
            c.G = g;
            c.B = b;
            c.A = a;

            _circlesToDraw.Add(c);
        }
        public override void drawLine(int x, int y, int x2, int y2, int r, int g, int b, int a)
        {
            Line l = new Line();
            l.Sx = x;
            l.Sy = y;
            l.Ex = x2;
            l.Ey = y2;

            l.R = r;
            l.G = g;
            l.B = b;
            l.A = a;

            _linesToDraw.Add(l);
        }

        public override void display()
        {
            SDL_FRect sRect;
            SDL_FRect tRect;

            unsafe
            {
                SDL_Renderer* renderer = (SDL_Renderer*)_rend;

                foreach (Transform trans in _toDraw)
                {
                    if (trans.SpritePath == null)
                        continue;

                    var sprite = loadTexture(trans);
                    if (sprite == IntPtr.Zero)
                        continue;

                    SDL_Texture* texture = (SDL_Texture*)sprite;

                    sRect.x = 0;
                    sRect.y = 0;
                    sRect.w = (float)(trans.Wid * trans.Scalex);
                    sRect.h = (float)(trans.Ht * trans.Scaley);

                    tRect.x = (float)trans.X;
                    tRect.y = (float)trans.Y;
                    tRect.w = sRect.w;
                    tRect.h = sRect.h;

                    // This binding expects FRect*
                    SDL_RenderTextureRotated(renderer, texture, &sRect, &tRect, (double)trans.Rotz, null, 0);
                }

                foreach (Circle c in _circlesToDraw)
                {
                    SDL_SetRenderDrawColor(renderer, (byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
                    renderCircle(c.X, c.Y, c.Radius);
                }

                foreach (Line l in _linesToDraw)
                {
                    SDL_SetRenderDrawColor(renderer, (byte)l.R, (byte)l.G, (byte)l.B, (byte)l.A);
                    SDL_RenderLine(renderer, l.Sx, l.Sy, l.Ex, l.Ey);
                }
            }

            base.display();
        }



        public override void clearDisplay()
        {

            _toDraw.Clear();
            _circlesToDraw.Clear();
            _linesToDraw.Clear();

            base.clearDisplay();
        }

    }


}
