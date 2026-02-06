/*
*
*   SDL provides an input layer, and we're using that.  This class tracks input, anchors it to the 
*       timing of the game loop, and converts the SDL events into one that is more abstract so games 
*       can be written more interchangeably.
*   @author Michael Heron
*   @version 1.0
*   
*/
using SDL;
using static SDL.SDL3;

namespace Shard
{

    // We'll be using SDL2 here to provide our underlying input system.
    class InputFramework : InputSystem
    {

        double tick, timeInterval;
        public override unsafe void getInput()
        {
            SDL_Event ev;
            InputEvent ie;

            tick += Bootstrap.getDeltaTime();

            if (tick < timeInterval)
                return;

            while (tick >= timeInterval)
            {
                if (!SDL_PollEvent(&ev))
                {
                    return;
                }


                ie = new InputEvent();

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_MOTION)
                {
                    SDL_MouseMotionEvent mot = ev.motion;
                    ie.X = (int)mot.x;
                    ie.Y = (int)mot.y;
                    informListeners(ie, "MouseMotion");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN)
                {
                    SDL_MouseButtonEvent butt = ev.button;
                    ie.Button = (int)butt.button;
                    ie.X = (int)butt.x;
                    ie.Y = (int)butt.y;
                    informListeners(ie, "MouseDown");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
                {
                    SDL_MouseButtonEvent butt = ev.button;
                    ie.Button = (int)butt.button;
                    ie.X = (int)butt.x;
                    ie.Y = (int)butt.y;
                    informListeners(ie, "MouseUp");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_WHEEL)
                {
                    SDL_MouseWheelEvent wh = ev.wheel;
                    ie.X = (int)wh.x;
                    ie.Y = (int)wh.y;
                    informListeners(ie, "MouseWheel");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_KEY_DOWN)
                {
                    ie.Key = (int)ev.key.scancode;
                    Debug.getInstance().log("Keydown: " + ie.Key);
                    informListeners(ie, "KeyDown");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_KEY_UP)
                {
                    ie.Key = (int)ev.key.scancode;
                    informListeners(ie, "KeyUp");
                }

            }
        }

        public override void initialize()
        {
            tick = 0;
            timeInterval = 1.0 / 60.0;
        }

    }
}