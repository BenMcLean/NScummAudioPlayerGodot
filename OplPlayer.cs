﻿using Godot;
using NScumm.Core.Audio.OPL;
using System;
using System.Threading;

public class OplPlayer : AudioStreamPlayer
{
    public OplPlayer()
    {
        Name = "OplPlayer";
        Stream = new AudioStreamGenerator()
        {
            MixRate = 44100,
            BufferLength = 0.05f, // Keep this as short as possible to minimize latency
        };
    }

    public int MixRate
    {
        get => (int)((AudioStreamGenerator)Stream).MixRate;
        set
        {
            ((AudioStreamGenerator)Stream).MixRate = value;
            Opl?.Init(MixRate);
        }
    }

    public IMusicPlayer MusicPlayer { get; set; } = null;

    public IOpl Opl
    {
        get => opl;
        set
        {
            if ((opl = value) != null)
            {
                Opl.Init((int)((AudioStreamGenerator)Stream).MixRate);
            }
        }
    }
    private IOpl opl;

    public override void _Ready() => Play();

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (Playing && (thread == null || !thread.IsAlive))
        { // Create thread if it ever crashes
            thread = new System.Threading.Thread(new ThreadStart(AudioPlayerThread))
            {
                IsBackground = true,
            };
            thread.Start();
        }
    }

    private System.Threading.Thread thread = null;

    private void AudioPlayerThread()
    {
        while (Playing)
        {
            if (Opl == null)
                Stop();
            else
                FillBuffer();
            System.Threading.Thread.Sleep(10); // Sleep 10 msec periodically
        }
    }

    public OplPlayer FillBuffer()
    {
        if (Opl == null)
            return this;
        int toFill = ((AudioStreamGeneratorPlayback)GetStreamPlayback()).GetFramesAvailable();
        if (ShortBuffer == null || ShortBuffer.Length < toFill)
            ShortBuffer = new short[toFill];
        int pos = 0;
        void FillBuffer2()
        {
            int i;
            while (toFill > 0)
            {
                while (minicnt < 0)
                {
                    minicnt += MixRate;
                    if (!MusicPlayer.Update())
                        return;
                }
                i = Math.Min(toFill, (int)(minicnt / MusicPlayer.RefreshRate + 4) & ~3);
                Opl.ReadBuffer(ShortBuffer, pos, i);
                pos += i;
                toFill -= i;
                minicnt -= (int)(MusicPlayer.RefreshRate * i);
            }
        }
        FillBuffer2();
        if (pos > 0)
        {
            Vector2[] vector2Buffer = new Vector2[pos];
            for (uint i = 0; i < vector2Buffer.Length; i++)
            {
                float soundbite = ShortBuffer[i] / 32767f; // Convert from 16 bit signed integer audio to 32 bit signed float audio
                vector2Buffer[i] = new Vector2(soundbite, soundbite); // Convert mono to stereo
            }
            if (vector2Buffer.Length > 0)
                ((AudioStreamGeneratorPlayback)GetStreamPlayback()).PushBuffer(vector2Buffer);
        }
        return this;
    }
    private short[] ShortBuffer;
    private int minicnt = 0;
}
