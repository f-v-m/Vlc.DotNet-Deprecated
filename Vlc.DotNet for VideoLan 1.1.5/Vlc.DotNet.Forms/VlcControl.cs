﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Helpers;
using Vlc.DotNet.Core.Interop;
using Vlc.DotNet.Core.Medias;

namespace Vlc.DotNet.Forms
{
    /// <summary>
    /// Vlc WinForms Control
    /// </summary>
    [DefaultProperty("Manager")]
    public sealed class VlcControl : Control
    {
        private IntPtr myEventCallback;
        private bool myIsDisposed;
        private VlcManager myVlcManager;

        public VlcControl()
        {
            BackColor = Color.Black;
        }

        internal IntPtr VlcMediaPlayer { get; private set; }
        internal IntPtr VlcMediaPlayerEventManager { get; private set; }

        [DefaultValue((string) null)]
        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public VlcManager Manager
        {
            get { return myVlcManager; }
            set
            {
                if (myVlcManager == value)
                    return;
                if (myVlcManager != null)
                    myVlcManager.Dispose();
                myVlcManager = value;
            }
        }

        /// <summary>
        /// Get movie fps rate
        /// </summary>
        [Browsable(false)]
        public float FramesPerSecond
        {
            get
            {
                if (VlcMediaPlayer != IntPtr.Zero)
                    return LibVlcMethods.libvlc_media_player_get_fps(VlcMediaPlayer);
                return 0;
            }
        }

        /// <summary>
        /// Can this media player be paused?
        /// </summary>
        [Browsable(false)]
        public bool CanPause
        {
            get
            {
                if (VlcMediaPlayer != IntPtr.Zero)
                    return LibVlcMethods.libvlc_media_player_can_pause(VlcMediaPlayer) == 1;
                return false;
            }
        }

        /// <summary>
        /// Is this media player playing?
        /// </summary>
        [Browsable(false)]
        public bool IsPlaying
        {
            get
            {
                if (VlcMediaPlayer != IntPtr.Zero)
                    return LibVlcMethods.libvlc_media_player_is_playing(VlcMediaPlayer) == 1;
                return false;
            }
        }

        /// <summary>
        /// Is this media player seekable?
        /// </summary>
        [Browsable(false)]
        public bool IsSeekable
        {
            get
            {
                if (VlcMediaPlayer != IntPtr.Zero)
                    return LibVlcMethods.libvlc_media_player_is_seekable(VlcMediaPlayer) == 1;
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (myIsDisposed)
                return;

            if (VlcMediaPlayerEventManager != IntPtr.Zero)
            {
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerBackward, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerBuffering, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerEncounteredError, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerEndReached, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerForward, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerLengthChanged, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerMediaChanged, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerOpening, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerPausableChanged, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerPaused, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerPositionChanged, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerSeekableChanged, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerSnapshotTaken, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerStopped, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerTimeChanged, myEventCallback, IntPtr.Zero);
                LibVlcMethods.libvlc_event_detach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerTitleChanged, myEventCallback, IntPtr.Zero);
            }

            if (VlcMediaPlayer != IntPtr.Zero)
            {
                Stop();
                var media = LibVlcMethods.libvlc_media_player_get_media(VlcMediaPlayer);
                if (media != IntPtr.Zero)
                    LibVlcMethods.libvlc_media_release(media);

                LibVlcMethods.libvlc_media_player_release(VlcMediaPlayer);
                VlcMediaPlayer = IntPtr.Zero;
            }
            myIsDisposed = true;
        }

        ~VlcControl()
        {
            Dispose(false);
        }

        private void InitVlcMediaPlayer()
        {
            if (DesignMode)
                return;

            if (myVlcManager == null || myVlcManager.VlcClient == IntPtr.Zero || VlcMediaPlayer != IntPtr.Zero)
                return;

            VlcMediaPlayer = LibVlcMethods.libvlc_media_player_new(myVlcManager.VlcClient);
            LibVlcMethods.libvlc_media_player_set_hwnd(VlcMediaPlayer, Handle);
            VlcMediaPlayerEventManager = LibVlcMethods.libvlc_media_player_event_manager(VlcMediaPlayer);
            if (VlcMediaPlayerEventManager == IntPtr.Zero)
                return;
            LibVlcMethods.EventCallbackDelegate callback = OnVlcEvent;
            myEventCallback = Marshal.GetFunctionPointerForDelegate(callback);
            GC.KeepAlive(callback);

            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerBackward, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerBuffering, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerEncounteredError, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerEndReached, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerForward, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerLengthChanged, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerMediaChanged, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerOpening, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerPausableChanged, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerPaused, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerPositionChanged, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerSeekableChanged, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerSnapshotTaken, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerStopped, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerTimeChanged, myEventCallback, IntPtr.Zero);
            LibVlcMethods.libvlc_event_attach(VlcMediaPlayerEventManager, libvlc_event_e.MediaPlayerTitleChanged, myEventCallback, IntPtr.Zero);
        }

        public void Play(MediaBase media)
        {
            if (DesignMode)
                return;

            //Initialize a default VlcManager
            if (myVlcManager == null)
                myVlcManager = new VlcManager();
            InitVlcMediaPlayer();
            if (media.Initialize(myVlcManager.VlcClient) == IntPtr.Zero)
                return;

            LibVlcMethods.libvlc_media_player_set_media(VlcMediaPlayer, media.VlcMedia);
            LibVlcMethods.libvlc_media_player_play(VlcMediaPlayer);
        }

        public void Stop()
        {
            if (DesignMode)
                return;
            if (VlcMediaPlayer == IntPtr.Zero)
                return;
            LibVlcMethods.libvlc_media_player_stop(VlcMediaPlayer);
        }

        public void Pause()
        {
            if (DesignMode)
                return;
            if (VlcMediaPlayer == IntPtr.Zero)
                return;
            LibVlcMethods.libvlc_media_player_pause(VlcMediaPlayer);
        }

        public void TakeSnapshot(string path = null, uint width = (uint) 300, uint height = (uint) 300)
        {
            if (DesignMode)
                return;
            if (VlcMediaPlayer == IntPtr.Zero)
                return;
            if (Directory.Exists(path ?? AppDomain.CurrentDomain.BaseDirectory))
                LibVlcMethods.libvlc_video_take_snapshot(VlcMediaPlayer, 0, path ?? AppDomain.CurrentDomain.BaseDirectory, width, height);
            else
                throw new DirectoryNotFoundException();
        }

        #region Events

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<EventArgs> Backward;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<float> Buffering;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<EventArgs> EncounteredError;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<EventArgs> EndReached;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<EventArgs> Forward;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<long> LengthChanged;

        //TODO
        //[Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        //public event VlcEventHandler<MediaBase> MediaChanged;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<int> PausableChanged;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<EventArgs> Paused;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<float> PositionChanged;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<int> SeekableChanged;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<string> SnapshotTaken;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<EventArgs> Stopped;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<long> TimeChanged;

        [Category(CommonStrings.VLC_DOTNET_PROPERTIES_CATEGORY)]
        public event VlcEventHandler<long> TitleChanged;

        private void OnVlcEvent(ref libvlc_event_t libvlc_event, IntPtr userdata)
        {
            switch (libvlc_event.type)
            {
                case libvlc_event_e.MediaPlayerBackward:
                    EventsHelper.RaiseEvent(Backward, this, new VlcEventArgs<EventArgs>(EventArgs.Empty));
                    break;
                case libvlc_event_e.MediaPlayerBuffering:
                    EventsHelper.RaiseEvent(Buffering, this, new VlcEventArgs<float>(libvlc_event.media_player_buffering.new_cache));
                    break;
                case libvlc_event_e.MediaPlayerEncounteredError:
                    EventsHelper.RaiseEvent(EncounteredError, this, new VlcEventArgs<EventArgs>(EventArgs.Empty));
                    break;
                case libvlc_event_e.MediaPlayerEndReached:
                    EventsHelper.RaiseEvent(EndReached, this, new VlcEventArgs<EventArgs>(EventArgs.Empty));
                    break;
                case libvlc_event_e.MediaPlayerForward:
                    EventsHelper.RaiseEvent(Forward, this, new VlcEventArgs<EventArgs>(EventArgs.Empty));
                    break;
                case libvlc_event_e.MediaPlayerLengthChanged:
                    EventsHelper.RaiseEvent(LengthChanged, this, new VlcEventArgs<long>(libvlc_event.media_player_length_changed.new_length));
                    break;
                case libvlc_event_e.MediaPlayerMediaChanged:
                    //TODO
                    //EventsHelper.RaiseEvent(MediaChanged, this, new VlcEventArgs<MediaBase>(libvlc_event.media_player_media_changed.new_media));
                    break;
                case libvlc_event_e.MediaPlayerNothingSpecial:

                    break;
                case libvlc_event_e.MediaPlayerOpening:
                    break;
                case libvlc_event_e.MediaPlayerPausableChanged:
                    EventsHelper.RaiseEvent(PausableChanged, this, new VlcEventArgs<int>(libvlc_event.media_player_pausable_changed.new_pausable));
                    break;
                case libvlc_event_e.MediaPlayerPaused:
                    EventsHelper.RaiseEvent(Paused, this, new VlcEventArgs<EventArgs>(EventArgs.Empty));
                    break;
                case libvlc_event_e.MediaPlayerPositionChanged:
                    EventsHelper.RaiseEvent(PositionChanged, this, new VlcEventArgs<float>(libvlc_event.media_player_position_changed.new_position));
                    break;
                case libvlc_event_e.MediaPlayerSeekableChanged:
                    EventsHelper.RaiseEvent(SeekableChanged, this, new VlcEventArgs<int>(libvlc_event.media_player_seekable_changed.new_seekable));
                    break;
                case libvlc_event_e.MediaPlayerSnapshotTaken:
                    EventsHelper.RaiseEvent(SnapshotTaken, this, new VlcEventArgs<string>(IntPtrExtensions.ToStringAnsi(libvlc_event.media_player_snapshot_taken.psz_filename)));
                    break;
                case libvlc_event_e.MediaPlayerStopped:
                    EventsHelper.RaiseEvent(Stopped, this, new VlcEventArgs<EventArgs>(EventArgs.Empty));
                    break;
                case libvlc_event_e.MediaPlayerTimeChanged:
                    EventsHelper.RaiseEvent(TimeChanged, this, new VlcEventArgs<long>(libvlc_event.media_player_time_changed.new_time));
                    break;
                case libvlc_event_e.MediaPlayerTitleChanged:
                    EventsHelper.RaiseEvent(TitleChanged, this, new VlcEventArgs<long>(libvlc_event.media_player_title_changed.new_title));
                    break;
            }
        }

        #endregion
    }
}