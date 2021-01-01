using Godot;
using NScumm.Audio.OPL.Woody;
using NScumm.Core.Audio.OPL;
using System.IO;
using WOLF3DModel;

public class Main : Node2D
{
    public OplPlayer OplPlayer;
    public override void _Ready()
    {
        AddChild(OplPlayer = new OplPlayer()
        {
            Opl = new WoodyEmulatorOpl(OplType.Opl2),
        });
        string imfFile = "SEARCHN_MUS.imf";
        if (!System.IO.File.Exists(imfFile))
            throw new FileNotFoundException();
        else
            using (FileStream imfStream = new FileStream(imfFile, FileMode.Open))
                OplPlayer.ImfPlayer.Imf = Imf.ReadImf(imfStream);
    }
}
