program ProjClient;

uses
  Forms,
  UnitClient in 'UnitClient.pas' {SimpleClient};

{$R *.res}

begin
  Application.Initialize;
  Application.CreateForm(TSimpleClient, SimpleClient);
  Application.Run;
end.
