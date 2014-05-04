unit UnitClient;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, ScktComp, ExtCtrls;

const FIELD_LENGTH = 9;      //length of the the soccer field
const FIELD_WIDTH  = 6;      //width of the soccer field
const GAME_LENGTH  = 10000; //Number of game cycles

type TPosition = record
  Row : Integer;
  Col : Integer;
end;

type TGameState = ( GSPlayOn, //The Game is running
                    GSGoalLeft, //Left player scores
                    GSGoalRight, //Right player scores
                    GSFinished   // End of the game
                  );

type TState = record
  MyPos : TPosition;
  OppPos : TPosition;
  BallOwner : Char;
  GameState : TGameState;
  CurrentCycle : Cardinal;
  MyScore : Cardinal;
  OppScore : Cardinal;
end;

type
  TSimpleClient = class(TForm)
    Label1: TLabel;
    Button2: TButton;
    EditPort: TEdit;
    Label2: TLabel;
    EditName: TEdit;
    Label3: TLabel;
    EditIP: TEdit;
    Label4: TLabel;
    Label5: TLabel;
    Label6: TLabel;
    Label7: TLabel;
    Label8: TLabel;
    LabelMyPos: TLabel;
    LabelOppPos: TLabel;
    LabelScore: TLabel;
    LabelBallOwner: TLabel;
    LabelCurrentCycle: TLabel;
    Label9: TLabel;
    LabelGameState: TLabel;
    Bevel1: TBevel;
    Bevel2: TBevel;
    Button1: TButton;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure Button2Click(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure Button1Click(Sender: TObject);
  private
    ServerMsg : String;
    ClientSocket: TClientSocket;
    CurrentState : TState;

    procedure OnRead( Sender: TObject; Socket: TCustomWinSocket );
    procedure updateState;
    function GetCommand : String;
  public
    { Public declarations }
  end;

var
  SimpleClient: TSimpleClient;

implementation

uses StrUtils, DateUtils;

{$R *.dfm}

procedure TSimpleClient.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  try
    ClientSocket.Close;
    ClientSocket.Active := False;
    ClientSocket.Destroy;
  except
  end;
end;

procedure TSimpleClient.Button2Click(Sender: TObject);
begin
  ClientSocket.Port := StrToInt( EditPort.Text );
  ClientSocket.Host := EditIP.Text;
  try
    ClientSocket.Active := True;
  except
    ShowMessage( 'Cannot connect to the server' );
    Halt;
  end;
  Button2.Enabled := False;
  Button1.Enabled := True;
end;

procedure TSimpleClient.FormCreate(Sender: TObject);
begin
  Randomize;
  ClientSocket := TClientSocket.Create( Self );
  ClientSocket.OnRead := OnRead;

  //initialization values for CurrentState
  with CurrentState do
  begin
    MyPos.Row := 0;
    MyPos.Col := 0;
    OppPos := MyPos;
    MyScore := 0;
    OppScore := 0;
    CurrentCycle := 0;
    BallOwner := 'U';
    GameState := GSPlayOn;
  end;

  ServerMsg := '';
end;


procedure TSimpleClient.Button1Click(Sender: TObject);
begin
  if ClientSocket.Active then
    ClientSocket.Socket.SendText( 'I:' + EditName.Text )
  else
  begin
    ShowMessage( 'client is not active' );
    Halt;
  end;
end;

procedure TSimpleClient.OnRead( Sender: TObject; Socket: TCustomWinSocket );
begin
    ServerMsg := ClientSocket.Socket.ReceiveText;
    if ServerMsg = '' then
      Exit;

    UpdateState;
    ClientSocket.Socket.SendText( GetCommand );
end;

procedure TSimpleClient.UpdateState;
var
  TempStr : String;
begin
  TempStr := LeftStr( ServerMsg, 1 );
  CurrentState.MyPos.Row := StrToInt( TempStr );
  TempStr := MidStr( ServerMsg, 2, 1 );
  CurrentState.MyPos.Col := StrToInt( TempStr );
  TempStr := MidStr( ServerMsg, 3, 1 );
  CurrentState.OppPos.Row := StrToInt( TempStr );
  TempStr := MidStr( ServerMsg, 4, 1 );
  CurrentState.OppPos.Col := StrToInt( TempStr );
  if ServerMsg[ 5 ] = 'P' then
    CurrentState.BallOwner := 'P'
  else if ServerMsg[ 5 ] = 'O' then
    CurrentState.BallOwner := 'O'
  else
    CurrentState.BallOwner := 'U'; //unknown character
  case ServerMsg[ 6 ] of
    'C': CurrentState.GameState := GSPlayOn;
    'L': CurrentState.GameState := GSGoalLeft; //goal for me
    'R': CurrentState.GameState := GSGoalRight; //goal for opponent
    'F': CurrentState.GameState := GSFinished;
  end;
  TempStr := MidStr( ServerMsg, 7, 10 );
  CurrentState.CurrentCycle := StrToInt( TempStr );
  TempStr := MidStr( ServerMsg, 17, 5 );
  CurrentState.MyScore := StrToInt( TempStr );
  TempStr := MidStr( ServerMsg, 22, 5 );
  CurrentState.OppScore := StrToInt( TempStr );
end;

function TSimpleClient.GetCommand : String;
var
  RandInt : Integer;
begin
  //Random Command
  RandInt := Random( 5 ); //A number between 0 to 4
  (*
    0: Hold(Stand)
    1: North
    2: South
    3: West
    4: East
  *)
  Result := IntToStr( RandInt );

  ////////////////////////////////////
  ////////////////////////////////////
  ////////////////////////////////////
  (* Put your code in this function *)
  ////////////////////////////////////
  ////////////////////////////////////
  ////////////////////////////////////
end;


end.
