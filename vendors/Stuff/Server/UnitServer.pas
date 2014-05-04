unit UnitServer;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Scktcomp, StdCtrls, UnitSServer, ExtCtrls, Spin;

type
  TForm1 = class(TForm)
    Button1: TButton;
    Memo1: TMemo;
    Label1: TLabel;
    Edit1: TEdit;
    Timer: TTimer;
    Label2: TLabel;
    SpinWait: TSpinEdit;
    Label3: TLabel;
    EditLogFile: TEdit;
    Button2: TButton;
    SaveDialog: TSaveDialog;
    procedure Button1Click(Sender: TObject);
    procedure OnClientRead( Sender: TObject; Socket: TCustomWinSocket );
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure TimerTimer(Sender: TObject);
    procedure SpinWaitChange(Sender: TObject);
    procedure Button2Click(Sender: TObject);
  private
    server : TServerSocket;

    PosPlayerLeft, PosPlayerRight : TPosition;
    ScoreLeft, ScoreRight : Cardinal;
    sserver : TSServer;
    IsSServerInit : Boolean;

    PortLeft, PortRight, PortMonitor : Integer;
    IPLeft, IPRight, IPMonitor : String;
    PlayerLeftName, PlayerRightName : String;
    IsLeftInit, IsRightInit, IsMonitorInit, IsMonitorFirstMsg : Boolean;

    PlayerLeftCommand, PlayerRightCommand : TCommand;
    MsgLeft, MsgRight, MsgMonitor : String;
    IsLeftSendCommand, IsRightSendCommand : Boolean;
    FileHandle : TextFile;

    procedure CreateMessages;
    procedure SendMessages;
    procedure TerminateActions;
  public
    { Public declarations }
  end;

var
  Form1: TForm1;

implementation

uses DateUtils, StrUtils;

{$R *.dfm}

procedure TForm1.Button1Click(Sender: TObject);
begin
  PortLeft := -1;
  PortRight := -1;
  PortMonitor := -1;
  IsLeftInit := False;
  IsRightInit := False;
  IsMonitorInit := False;
  IsMonitorFirstMsg := True;
  PlayerLeftName := '';
  PlayerRightName := '';
  PlayerLeftCommand := Hold;
  PlayerRightCommand := Hold;
  IsLeftSendCommand := False;
  IsRightSendCommand := False;

  IsSServerInit := false;

  server.Port := StrToInt( Edit1.Text );
  server.Active := True;
  AssignFile( FileHandle, EditLogFile.Text );
  Rewrite( FileHandle );
  timer.Interval := SpinWait.Value;
  Timer.Enabled := True;
end;

procedure TForm1.OnClientRead( Sender: TObject; Socket: TCustomWinSocket );
var
  i : integer;
  RecText, TempStr : string;
begin
  for i := 0 to server.Socket.ActiveConnections - 1 do
  begin
    try
    with server.Socket.Connections[ i ] do
    begin
      RecText := ReceiveText;
      if RecText = '' then
        Continue;

      TempStr := LeftStr( RecText, 1 );
      if UpperCase( TempStr ) = 'I' then //a player or monitor tries to connect to server
      begin
        TempStr := MidStr( RecText, 3, Length( RecText ) - 2 );

        if (not IsMonitorInit) AND (TempStr = '__MONITOR__') then
        begin
          PortMonitor := RemotePort;
          IPMonitor := RemoteAddress;
          IsMonitorInit := True;
          Memo1.Lines.Add( 'Monitor connected ('
                            + IPMonitor + ':'
                            + IntToStr( PortMonitor )
                            + ' )' );

        end
        else if not IsLeftInit then
        begin
          PortLeft := RemotePort;
          IPLeft := RemoteAddress;
          PlayerLeftName := TempStr;
          IsLeftInit := True;
          Memo1.Lines.Add( 'Player ' + PlayerLeftName + ' connected on side LEFT ('
                            + IPLeft + ':'
                            + IntToStr( PortLeft )
                            + ' )' );
        end
        else if not IsRightInit then
        begin
          PortRight := RemotePort;
          IPRight := RemoteAddress;
          PlayerRightName := TempStr;
          IsRightInit := True;
          Memo1.Lines.Add( 'Player ' + PlayerRightName + ' connected on side RIGHT ('
                            + IPRight + ':'
                            + IntToStr( PortRight )
                            + ' )' );
        end;

        if IsLeftInit AND IsRightInit AND (not IsSServerInit) then
        begin
          Writeln( FileHandle, PlayerLeftName );
          WriteLn( FileHandle, PlayerRightName );
          sserver := TSServer.Create( PlayerLeftName, PlayerRightName );
          IsSServerInit := True;
        end;
      end
      else //Command from players
      begin
        if (RemotePort = PortLeft) AND (RemoteAddress = IPLeft) then //Command from left player
        begin
          IsLeftSendCommand := True;
          case StrToInt( TempStr ) of
            0: PlayerLeftCommand := Hold;
            1: PlayerLeftCommand := North;
            2: PlayerLeftCommand := South;
            3: PlayerLeftCommand := West;
            4: PlayerLeftCommand := East;
            else
              IsLeftSendCommand := False;
          end;
        end
        else if (RemotePort = PortRight) AND (RemoteAddress = IPLeft) then //command from right player
        begin
          IsRightSendCommand := True;
          case StrToInt( TempStr ) of
            0: PlayerRightCommand := Hold;
            1: PlayerRightCommand := North;
            2: PlayerRightCommand := South;
            3: PlayerRightCommand := West;
            4: PlayerRightCommand := East;
            else
              IsRightSendCommand := False;
          end;
        end;
      end;
    end;
    except
    end;
  end; //end of for

end;

procedure TForm1.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  TerminateActions;
end;

procedure TForm1.FormCreate(Sender: TObject);
begin
  server := TServerSocket.Create( Self );
  server.OnClientRead := OnClientRead;
end;

procedure TForm1.TimerTimer(Sender: TObject);
begin
  if IsSServerInit then
  begin
    if IsLeftSendCommand then
    begin
      sserver.SetCommandPlayerLeft( PlayerLeftCommand );
      IsLeftSendCommand := False;
    end
    else
    begin
      PlayerLeftCommand := sserver.GetRandomCommand;
      sserver.SetCommandPlayerLeft( PlayerLeftCommand );
    end;

    if IsRightSendCommand then
    begin
      sserver.SetCommandPlayerRight( PlayerRightCommand );
      IsRightSendCommand := False;
    end
    else
    begin
      PlayerRightCommand := sserver.GetRandomCommand;
      sserver.SetCommandPlayerRight( PlayerRightCommand );
    end;

    sserver.PerformCommands;
    if sserver.GetGameState = GSFinished then
      TerminateActions;

    CreateMessages;
    Writeln( FileHandle, MsgLeft );
    SendMessages;
  end;
end;

procedure TForm1.CreateMessages;
var
  CycleStr, TempStr : String;
begin
  PosPlayerLeft := sserver.GetPositionPlayerLeft;
  PosPlayerRight := sserver.GetPositionPlayerRight;
  ScoreLeft := sserver.GetScorePlayerLeft;
  ScoreRight := sserver.GetScorePlayerRight;

  //Creating Player Left Message
  MsgLeft := IntToStr( PosPlayerLeft.Row )
           + IntToStr( PosPlayerLeft.Col )
           + IntToStr( PosPlayerRight.Row )
           + IntToStr( PosPlayerRight.Col );
  if sserver.GetBallOwnerSide = SLeft then
    MsgLeft := MsgLeft + 'P'
  else
    MsgLeft := MsgLeft + 'O';
  case sserver.GetGameState of
    GSPlayOn: MsgLeft := MsgLeft + 'C';  //Play On
    GSGoalLeft: MsgLeft := MsgLeft + 'L'; //Goal Left
    GSGoalRight: MsgLeft := MsgLeft + 'R'; //Goal Right
    GSFinished: MsgLeft := MsgLeft + 'F';  //Finished Game
  end;
  CycleStr := '';
  Str( sserver.GetCurrentCycle, CycleStr );
  while Length( CycleStr ) < 10 do
    CycleStr := '0' + CycleStr;
  MsgLeft := MsgLeft + CycleStr;
  TempStr := '';
  Str( ScoreLeft, TempStr );
  while Length( TempStr ) < 5 do
    TempStr := '0' + TempStr;
  MsgLeft := MsgLeft + TempStr;
  TempStr := '';
  Str( ScoreRight, TempStr );
  while Length( TempStr ) < 5 do
    TempStr := '0' + TempStr;
  MsgLeft := MsgLeft + TempStr;

  //Creating Player Right Message
  PosPlayerLeft := sserver.InversePos( PosPlayerLeft );
  PosPlayerRight := sserver.InversePos( PosPlayerRight );
  MsgRight := IntToStr( PosPlayerRight.Row )
           + IntToStr( PosPlayerRight.Col )
           + IntToStr( PosPlayerLeft.Row )
           + IntToStr( PosPlayerLeft.Col );
  if sserver.GetBallOwnerSide = SRight then
    MsgRight := MsgRight + 'P'
  else
    MsgRight := MsgRight + 'O';
  case sserver.GetGameState of
    GSPlayOn: MsgRight := MsgRight + 'C';  //Play On
    GSGoalRight: MsgRight := MsgRight + 'L'; //Goal Left
    GSGoalLeft: MsgRight := MsgRight + 'R'; //Goal Right
    GSFinished: MsgRight := MsgRight + 'F';  //Finished Game
  end;
  MsgRight := MsgRight + CycleStr;
  TempStr := '';
  Str( ScoreRight, TempStr );
  while Length( TempStr ) < 5 do
    TempStr := '0' + TempStr;
  MsgRight := MsgRight + TempStr;
  TempStr := '';
  Str( ScoreLeft, TempStr );
  while Length( TempStr ) < 5 do
    TempStr := '0' + TempStr;
  MsgRight := MsgRight + TempStr;

  //Creating Monitor Message if there is a monitor
  if IsMonitorInit then
  begin
    MsgMonitor := MsgLeft;
  end;

end;

procedure TForm1.SendMessages;
var
  i : Integer;
begin
  for i := 0 to server.Socket.ActiveConnections - 1 do
  begin
    with server.Socket.Connections[ i ] do
    begin
      if (RemotePort = PortLeft) AND (RemoteAddress = IPLeft) then //Left Player
      begin
        try
          SendText( MsgLeft );
        except
        end;
      end
      else if (RemotePort = PortRight) AND (RemoteAddress = IPRight) then //Right Player
      begin
        try
          SendText( MsgRight )
        except
        end;
      end
      else if (IsMonitorInit) AND (RemotePort = PortMonitor) AND (RemoteAddress = IPMonitor) then //Connectec Monitor
      begin
        try
          if IsMonitorFirstMsg then
          begin
            IsMonitorFirstMsg := False;
            SendText( '_' + PlayerLeftName + '_' + PlayerRightName );
          end
          else
          begin
            SendText( MsgMonitor );
          end;
        except
        end;
      end;
    end;
  end;
end;

procedure TForm1.SpinWaitChange(Sender: TObject);
begin
  timer.Interval := SpinWait.Value;
end;

procedure TForm1.Button2Click(Sender: TObject);
begin
  if SaveDialog.Execute then
    EditLogFile.Text := SaveDialog.FileName;
end;

procedure TForm1.TerminateActions;
begin
  try
    CloseFile( FileHandle );
  except
  end;
  Timer.Enabled := False;
  server.Active := False;
  server.Destroy;
  if IsSServerInit then
    sserver.Destroy;

  Halt;
end;

end.
