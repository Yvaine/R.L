program LogPlayer;

uses
  Forms,
  UnitLogPlayer in 'UnitLogPlayer.pas' {FormLogPlayer};

{$R *.res}

begin
  Application.Initialize;
  Application.CreateForm(TFormLogPlayer, FormLogPlayer);
  Application.Run;
end.
