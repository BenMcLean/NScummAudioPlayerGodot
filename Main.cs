using Godot;
using NScumm.Audio.OPL.Woody;
using NScumm.Core.Audio.OPL;
using System;
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
		using (FileStream imfStream = new FileStream("SEARCHN_MUS.imf", FileMode.Open))
			((ImfPlayer)OplPlayer.Players[0]).Imf = Imf.ReadImf(imfStream);
	}
}
