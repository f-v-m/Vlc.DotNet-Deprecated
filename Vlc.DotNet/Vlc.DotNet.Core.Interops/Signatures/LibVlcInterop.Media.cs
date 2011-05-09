﻿using System;
using System.Runtime.InteropServices;

namespace Vlc.DotNet.Core.Interops.Signatures
{
    namespace LibVlc
    {
        namespace Media
        {
            #region LibVLC meta data

            public enum Metadatas
            {
                Title = 0,
                Artist,
                Genre,
                Copyright,
                Album,
                TrackNumber,
                Description,
                Rating,
                Date,
                Setting,
                URL,
                Language,
                NowPlaying,
                Publisher,
                EncodedBy,
                ArtworkURL,
                TrackID
            }

            #endregion

            #region LibVlc_Media Struct & enum

            public enum States
            {
                NothingSpecial = 0,
                Opening,
                Buffering,
                Playing,
                Paused,
                Stopped,
                Ended,
                Error
            }

            public enum Option
            {
                Trusted = 0x2,
                Unique = 0x100
            }

            public enum TrackTypes
            {
                TrackUnknown = -1,
                TrackAudio = 0,
                TrackVideo = 1,
                TrackText = 2
            }

            public struct Stats
            {
                /* Input */
                public int ReadBytes;
                public float InputBitrate;

                /* Demux */
                public int DemuxReadBytes;
                public float DemuxBitrate;
                public int DemuxCorrupted;
                public int DemuxDiscontinuity;

                /* Decoders */
                public int DecodedVideo;
                public int DecodedAudio;


                /* Video Output */
                public int DisplayedPictures;
                public int LostPictures;

                /* Audio output */
                public int PlayedAusioBuffers;
                public int LostAudioBuffers;

                /* Stream output */
                public int SentPackets;
                public int SentBytes;
                public float SendBitrate;
            }

            public struct MediaTrackInfo
            {
                /* Codec fourcc */
                public uint Codec;
                public int Id;
                public TrackTypes Type;
                public int Profile;
                public int Level;

                public AudioStruct Audio;
                public VideoStruct Video;
            }

            public struct AudioStruct
            {
                /* Audio specific */
                public uint Channels;
                public uint Rate;
            }

            public struct VideoStruct
            {
                /* Video specific */
                public uint Height;
                public uint Width;
            }

            #endregion

            [LibVlcFunction("libvlc_media_new_location")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr NewFromLocation(IntPtr instance, string mrl);

            [LibVlcFunction("libvlc_media_new_path")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr NewFromPath(IntPtr instance, string path);

            [LibVlcFunction("libvlc_media_new_fd", "1.1.5")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr NewFromFileDescriptor(IntPtr instance, int fileDescriptor);

            [LibVlcFunction("libvlc_media_new_as_node")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr NewEmpty(IntPtr instance, string name);

            /// <summary>
            /// Add an option to the media.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <param name="options">The options.</param>
            [LibVlcFunction("libvlc_media_add_option")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void AddOption(IntPtr mediaInstance, string options);

            [LibVlcFunction("libvlc_media_add_option_flag")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void AddOptionFlag(IntPtr mediaInstance, string options, Option flag);

            [LibVlcFunction("libvlc_media_retain")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void RetainInstance(IntPtr mediaInstance);

            [LibVlcFunction("libvlc_media_release")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ReleaseInstance(IntPtr mediaInstance);

            /// <summary>
            /// Get the media resource locator (mrl) from a media descriptor object.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <returns>String with mrl of media descriptor object.</returns>
            [LibVlcFunction("libvlc_media_get_mrl")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string GetMrl(IntPtr mediaInstance);

            /// <summary>
            /// Duplicate a media descriptor object.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <returns>The duplicated media descriptor object.</returns>
            [LibVlcFunction("libvlc_media_duplicate")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr Duplicate(IntPtr mediaInstance);

            /// <summary>
            /// Read the meta of the media.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <param name="metadata">The meta to read.</param>
            /// <returns>The media's meta value.</returns>
            [LibVlcFunction("libvlc_media_get_meta")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string GetMetadata(IntPtr mediaInstance, Metadatas metadata);

            /// <summary>
            /// Set the meta of the media (this function will not save the meta, call SaveMetadatas in order to save the meta)
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <param name="metadata">The meta to write.</param>
            /// <param name="value">The media's meta value.</param>
            [LibVlcFunction("libvlc_media_set_meta")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SetMetadata(IntPtr mediaInstance, Metadatas metadata, string value);

            /// <summary>
            /// Save the meta previously set.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            [LibVlcFunction("libvlc_media_save_meta")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SaveMetadatas(IntPtr mediaInstance);

            /// <summary>
            /// Get current state of media descriptor object.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <returns>State of media descriptor object.</returns>
            [LibVlcFunction("libvlc_media_get_state")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate States GetState(IntPtr mediaInstance);

            /// <summary>
            /// Get the current statistics about the media.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <param name="stats">Structure that contain the statistics about the media.</param>
            /// <returns>True if the statistics are available, false otherwise.</returns>
            [LibVlcFunction("libvlc_media_get_stats")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int GetStats(IntPtr mediaInstance, Stats stats);

            //TODO : libvlc_media_subitems

            /// <summary>
            /// Get event manager from media descriptor object.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <returns>The event manager associated with mediaInstance.</returns>
            [LibVlcFunction("libvlc_media_event_manager")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr EventManager(IntPtr mediaInstance);

            /// <summary>
            /// Get duration (in ms) of media descriptor object item.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <returns>Duration of media item or -1 on error.</returns>
            [LibVlcFunction("libvlc_media_get_duration")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate long GetDuration(IntPtr mediaInstance);

            /// <summary>
            /// Parse a media.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            [LibVlcFunction("libvlc_media_parse")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Parse(IntPtr mediaInstance);

            /// <summary>
            /// Parse a media.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            [LibVlcFunction("libvlc_media_parse_async")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ParseAsync(IntPtr mediaInstance);

            /// <summary>
            /// Get Parsed status for media descriptor object.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <returns>True if media object has been parsed otherwise it returns false.</returns>
            [LibVlcFunction("libvlc_media_is_parsed")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int IsParsed(IntPtr mediaInstance);

            /// <summary>
            /// Sets media descriptor's user data.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <param name="userData">Pointer to user data.</param>
            [LibVlcFunction("libvlc_media_set_user_data")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SetUserData(IntPtr mediaInstance, IntPtr userData);

            /// <summary>
            /// Get media descriptor's user data.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <returns>Pointer to user data.</returns>
            [LibVlcFunction("libvlc_media_get_user_data")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr GetUserData(IntPtr mediaInstance);

            /// <summary>
            /// Get media descriptor's elementary streams description.
            /// </summary>
            /// <param name="mediaInstance">The media instance.</param>
            /// <param name="trackInfo">Tracks address to store an allocated array of Elementary Streams descriptions (must be freed by the caller).</param>
            /// <returns>Number of Elementary Streams.</returns>
            [LibVlcFunction("libvlc_media_get_tracks_info")]
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int GetTrackInfo(IntPtr mediaInstance, MediaTrackInfo trackInfo);
        }
    }
}