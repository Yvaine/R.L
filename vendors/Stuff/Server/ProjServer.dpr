program ProjServer;

uses
  Forms,
  UnitServer in 'UnitServer.pas' {Form1},
  UnitSServer in 'UnitSServer.pas';

{$R *.res}

begin
  Application.Initialize;
  Application.CreateForm(TForm1, Form1);
  Application.Run;
end.
