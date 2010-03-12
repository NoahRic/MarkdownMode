using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.Windows.Threading;

namespace MarkdownMode
{
    static class BufferIdleEventUtil
    {
        static object bufferListenersKey = new object();

        static Dictionary<ITextBuffer, DispatcherTimer> bufferTimers
            = new Dictionary<ITextBuffer, DispatcherTimer>();

        #region Public interface

        public static bool AddBufferIdleEventListener(ITextBuffer buffer, EventHandler handler)
        {
            HashSet<EventHandler> listenersForBuffer;
            if (!buffer.Properties.TryGetProperty(bufferListenersKey, out listenersForBuffer))
                listenersForBuffer = ConnectToBuffer(buffer);

            if (listenersForBuffer.Contains(handler))
                return false;

            listenersForBuffer.Add(handler);

            return true;
        }

        public static bool RemoveBufferIdleEventListener(ITextBuffer buffer, EventHandler handler)
        {
            HashSet<EventHandler> listenersForBuffer;
            if (!buffer.Properties.TryGetProperty(bufferListenersKey, out listenersForBuffer))
                return false;

            if (!listenersForBuffer.Contains(handler))
                return false;

            listenersForBuffer.Remove(handler);

            if (listenersForBuffer.Count == 0)
                DisconnectFromBuffer(buffer);

            return true;
        }

        #endregion

        #region Helpers

        static void DisconnectFromBuffer(ITextBuffer buffer)
        {
            buffer.Changed -= BufferChanged;

            DispatcherTimer timer;
            if (bufferTimers.TryGetValue(buffer, out timer))
            {
                timer.Stop();
                bufferTimers.Remove(buffer);
            }

            buffer.Properties.RemoveProperty(bufferListenersKey);
        }

        static HashSet<EventHandler> ConnectToBuffer(ITextBuffer buffer)
        {
            buffer.Changed += BufferChanged;

            RestartTimerForBuffer(buffer);

            HashSet<EventHandler> listenersForBuffer = new HashSet<EventHandler>();
            buffer.Properties[bufferListenersKey] = listenersForBuffer;

            return listenersForBuffer;
        }

        static void RestartTimerForBuffer(ITextBuffer buffer)
        {
            DispatcherTimer timer;

            if (bufferTimers.TryGetValue(buffer, out timer))
            {
                timer.Stop();
            }
            else
            {
                timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };

                timer.Tick += (s, e) =>
                {
                    if (timer != null)
                        timer.Stop();

                    HashSet<EventHandler> handlers;
                    if (buffer.Properties.TryGetProperty(bufferListenersKey, out handlers))
                    {
                        foreach (var handler in handlers)
                        {
                            handler(buffer, new EventArgs());
                        }
                    }
                };

                bufferTimers[buffer] = timer;
            }

            timer.Start();
        }

        static void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            ITextBuffer buffer = sender as ITextBuffer;
            if (buffer == null)
                return;

            RestartTimerForBuffer(buffer);
        }

        #endregion
    }
}
