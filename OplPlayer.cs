using Godot;
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
            MixRate = 48000,
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

    public readonly IMusicPlayer[] Players = new IMusicPlayer[]
    {
            new ImfPlayer(),
            new IdAdlPlayer(),
    };
    public ImfPlayer ImfPlayer => (ImfPlayer)Players[0];
    public IdAdlPlayer IdAdlPlayer => (IdAdlPlayer)Players[1];

    public IOpl Opl
    {
        get => opl;
        set
        {
            if ((opl = value) != null)
            {
                Opl.Init((int)((AudioStreamGenerator)Stream).MixRate);
                ImfPlayer.Opl = Opl;
                IdAdlPlayer.Opl = Opl;
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
        int toFill = ((AudioStreamGeneratorPlayback)GetStreamPlayback()).GetFramesAvailable() * (Opl.IsStereo ? 2 : 1);
        if (Buffer.Length < toFill)
            Buffer = new short[toFill];

        void FillBuffer2()
        {
            int i, minicnt = 0, pos = 0;
            while (toFill > 0)
            {
                while (minicnt < 0)
                {
                    minicnt += MixRate;
                    if (!Players[0].Update())
                        return;
                }
                i = Math.Min(toFill, (int)(minicnt / Players[0].RefreshRate + 4) & ~3);
                Players[0].Opl.ReadBuffer(Buffer, pos, i);
                pos += i;
                toFill -= i;
                minicnt -= (int)(Players[0].RefreshRate * i);
            }
        }
        FillBuffer2();

        Vector2[] buffer = new Vector2[toFill];
        for (uint i = 0; i < toFill; i++)
        {
            float soundbite = Buffer[i] / 32767f; // Convert from 16 bit signed integer audio to 32 bit signed float audio
            buffer[i] = new Vector2(soundbite, soundbite);
        }
        ((AudioStreamGeneratorPlayback)GetStreamPlayback()).PushBuffer(buffer);
        return this;
    }
    private short[] Buffer = new short[70000];
}
