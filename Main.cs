using Godot;
using NScumm.Audio.OPL.Woody;
using NScumm.Core.Audio.OPL;
using System.IO;
using WOLF3DModel;

public class Main : Node2D
{
	public OplPlayer ImfOplPlayer;
	public OplPlayer IdAdlOplPlayer;
	public Adl Adl;
	public override void _Ready()
	{
		IOpl imfOpl = new WoodyEmulatorOpl(OplType.Opl2);
		AddChild(ImfOplPlayer = new OplPlayer()
		{
			Opl = imfOpl,
			MusicPlayer = new ImfPlayer()
			{
				Opl = imfOpl,
			},
		});

		IOpl idAdlOpl = new WoodyEmulatorOpl(OplType.Opl2);
		AddChild(IdAdlOplPlayer = new OplPlayer()
		{
			Opl = idAdlOpl,
			MusicPlayer = new IdAdlPlayer()
			{
				Opl = idAdlOpl,
			},
		});

		string imfFile = "SEARCHN_MUS.imf";
		if (!System.IO.File.Exists(imfFile))
			throw new FileNotFoundException();
		else
			using (FileStream imfStream = new FileStream(imfFile, FileMode.Open))
				((ImfPlayer)ImfOplPlayer.MusicPlayer).Imf = Imf.ReadImf(imfStream);

		string idAdlFile = "GETAMMOSND.adl";
		if (!System.IO.File.Exists(idAdlFile))
			throw new FileNotFoundException();
		else
			using (FileStream idAdlStream = new FileStream(idAdlFile, FileMode.Open))
				Adl = new Adl(idAdlStream);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey inputEventKey && inputEventKey.Pressed && !inputEventKey.Echo)
			switch (inputEventKey.Scancode)
			{
				case (uint)KeyList.Space:
					((IdAdlPlayer)IdAdlOplPlayer.MusicPlayer).Adl = Adl;
					break;
			}
	}
}
