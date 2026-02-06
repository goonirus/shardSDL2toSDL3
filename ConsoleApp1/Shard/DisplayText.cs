/*
*
*   The baseline functionality for getting text to work via SDL.   You could write your own text 
*       implementation (and we did that earlier in the course), but bear in mind DisplaySDL is built
*       upon this class.
*   @author Michael Heron
*   @version 1.0
*   
*   Contributions to the code made by others:
*   @author Kyle Agius (see Changelog for 1.3.0)
*/

using SDL;
using static SDL.SDL3;
using static SDL.SDL3_ttf;
using System;
using System.Collections.Generic;
using System.IO;

namespace Shard
{

    // We'll be using SDL2 here to provide our underlying graphics system.
    class TextDetails
    {
        string text;
        double x, y;
        SDL.SDL_Color col;
        int size;
        IntPtr font;
        IntPtr lblText;


        public TextDetails(string text, double x, double y, SDL.SDL_Color col, int spacing)
        {
            this.text = text;
            this.x = x;
            this.y = y;
            this.col = col;
            this.size = spacing;
        }

        public string Text
        {
            get => text;
            set => text = value;
        }
        public double X
        {
            get => x;
            set => x = value;
        }
        public double Y
        {
            get => y;
            set => y = value;
        }
        public SDL.SDL_Color Col
        {
            get => col;
            set => col = value;
        }
        public int Size
        {
            get => size;
            set => size = value;
        }
        public IntPtr Font { get => font; set => font = value; }
        public IntPtr LblText { get => lblText; set => lblText = value; }
    }

    class DisplayText : Display
    {
        protected IntPtr _window, _rend;
        uint _format;
        int _access;
        private List<TextDetails> myTexts;
        private Dictionary<string, IntPtr> fontLibrary;
        public override void clearDisplay()
        {
            unsafe
            {
                SDL_Renderer* renderer = (SDL_Renderer*)_rend;

                foreach (TextDetails td in myTexts)
                {
                    SDL_DestroyTexture((SDL_Texture*)td.LblText);
                }

                myTexts.Clear();

                SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                SDL_RenderClear(renderer);
            }
        }


        public IntPtr loadFont(string path, int size)
        {
            string key = path + "," + size;

            if (fontLibrary.ContainsKey(key))
            {
                return fontLibrary[key];
            }

            unsafe
            {
                byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(path + "\0");
                fixed (byte* pPath = utf8)
                {
                    fontLibrary[key] = (IntPtr)TTF_OpenFont(pPath, size);
                }
            }

            return fontLibrary[key];
        }

        private void update()
        {


        }
        private void draw()
        {
            unsafe
            {
                SDL_Renderer* renderer = (SDL_Renderer*)_rend;

                foreach (TextDetails td in myTexts)
                {
                    // skip empty
                    if (td.LblText == IntPtr.Zero)
                        continue;

                    SDL_FRect dst;
                    dst.x = (float)td.X;
                    dst.y = (float)td.Y;

                    float w = 0, h = 0;

                    SDL_Texture* texture = (SDL_Texture*)td.LblText;
                    SDL_GetTextureSize(texture, &w, &h);

                    dst.w = w;
                    dst.h = h;

                    SDL_RenderTexture(renderer, texture, null, &dst);
                }

                SDL_RenderPresent(renderer);
            }
        }



        public override void display()
        {

            update();
            draw();
        }

        public override void setFullscreen()
        {
            unsafe
            {
                SDL_Window* win = (SDL_Window*)_window;
                SDL_SetWindowFullscreen(win, true);
            }
        }

        public override void initialize()
        {
            fontLibrary = new Dictionary<string, IntPtr>();
            setSize(1280, 864);

            unsafe
            {
                SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO);

                TTF_Init();

                SDL_Window* win = SDL_CreateWindow(
                    "Shard Game Engine",
                    getWidth(),
                    getHeight(),
                    0
                );
                _window = (nint)win;

                SDL_Renderer* renderer = SDL_CreateRenderer(win, new Utf8String());
                _rend = (nint)renderer;

                // Blend mode / clear color
                SDL_SetRenderDrawBlendMode(renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);
                SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            }

            myTexts = new List<TextDetails>();
        }


        public override void showText(string text, double x, double y, int size, int r, int g, int b)
        {
            string ffolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts);
            IntPtr font = loadFont(ffolder + "\\calibri.ttf", size);

            SDL_Color col;
            col.r = (byte)r;
            col.g = (byte)g;
            col.b = (byte)b;
            col.a = 255;

            if (font == IntPtr.Zero)
            {
                Debug.getInstance().log("TTF_OpenFont failed: " + SDL_GetError());
                return;
            }

            TextDetails td = new TextDetails(text, x, y, col, size);
            td.Font = font;

            td.LblText = IntPtr.Zero;

            myTexts.Add(td);
        }

        public override void showText(char[,] text, double x, double y, int size, int r, int g, int b)
        {
            string str = "";
            int row = 0;

            for (int i = 0; i < text.GetLength(0); i++)
            {
                str = "";
                for (int j = 0; j < text.GetLength(1); j++)
                {
                    str += text[j, i];
                }


                showText(str, x, y + (row * size), size, r, g, b);
                row += 1;

            }

        }
    }
}
