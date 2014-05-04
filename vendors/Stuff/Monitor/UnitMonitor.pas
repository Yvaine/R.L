unit UnitMonitor;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, ExtCtrls, ScktComp, StrUtils;

type TPosition = record
  Row : Integer;
  Col : Integer;
end;

type
  TForm1 = class(TForm)
    ImField: TImage;
    ImBlank: TImage;
    ImLeftBall: TImage;
    ImLeft: TImage;
    ImRightBall: TImage;
    ImRight: TImage;
    Panel1: TPanel;
    Label1: TLabel;
    Label2: TLabel;
    LabelGameState: TLabel;
    LabelCycleNo: TLabel;
    Bevel1: TBevel;
    Label3: TLabel;
    Label4: TLabel;
    LabelLName: TLabel;
    LabelLeftPos: TLabel;
    LabelRightPos: TLabel;
    LabelScore: TLabel;
    LabelRName: TLabel;
    Bevel2: TBevel;
    Label5: TLabel;
    Label6: TLabel;
    EditPort: TEdit;
    EditIP: TEdit;
    Button2: TButton;
    Button3: TButton;
    Label7: TLabel;
    Label8: TLabel;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormShow(Sender: TObject);
    procedure Button2Click(Sender: TObject);
    procedure Button3Click(Sender: TObject);
  private
    PrevPosLeft, PrevPosRight : TPosition;
    PosLeft, PosRight : TPosition;
    ScoreLeft, ScoreRight : Cardinal;
    CurrentCycle : Cardinal;
    BallOwner : String;

    PlayerNamesReceived : Boolean;
    ServerMsg : String;
    ClientSocket1: TClientSocket;
    procedure OnRead( Sender: TObject; Socket: TCustomWinSocket );
    procedure RepaintField;

  public
    { Public declarations }
  end;

var
  Form1: TForm1;

implementation

uses Math;

{$R *.dfm}

procedure TForm1.RepaintField;
var
  Origin : TPoint;
begin
  Origin.X := 25;
  Origin.Y := 2;

  ImField.Canvas.Draw( (PrevPosLeft.Col - 1)* 23 + Origin.X, (PrevPosLeft.Row - 1)* 23 + Origin.Y, ImBlank.Picture.Bitmap );
  ImField.Canvas.Draw( (PrevPosRight.Col - 1)* 23 + Origin.X, (PrevPosRight.Row - 1)* 23 + Origin.Y, ImBlank.Picture.Bitmap );

  if BallOwner = 'L' then
  begin
    ImField.Canvas.Draw( (PosLeft.Col - 1)*23 + Origin.X, (PosLeft.Row - 1)*23 + Origin.Y, ImLeftBall.Picture.Bitmap );
    ImField.Canvas.Draw( (PosRight.Col - 1)*23 + Origin.X, (PosRight.Row - 1)*23 + Origin.Y, ImRight.Picture.Bitmap );
  end
  else
  begin
    ImField.Canvas.Draw( (PosLeft.Col - 1)*23 + Origin.X, (PosLeft.Row - 1)*23 + Origin.Y, ImLeft.Picture.Bitmap );
    ImField.Canvas.Draw( (PosRight.Col - 1)*23 + Origin.X, (PosRight.Row - 1)*23 + Origin.Y, ImRightBall.Picture.Bitmap );
  end;

  PrevPosLeft := PosLeft;
  PrevPosRight := PosRight;

  if PlayerNamesReceived then
  begin
    case ServerMsg[ 6 ] of
      'C': LabelGameState.Caption := 'Play On';
      'L': LabelGameState.Caption := 'Goal Left';
      'R': LabelGameState.Caption := 'Goal Right';
      'F': LabelGameState.Caption := 'End of Game';
    end;
  end;
  LabelCycleNo.Caption := IntToStr( CurrentCycle );
  LabelLeftPos.Caption := '(' +
                          IntToStr( PosLeft.Row ) +
                          ',' +
                          IntToStr( PosLeft.Col ) +
                          ')';
  LabelRightPos.Caption := '(' +
                          IntToStr( PosRight.Row ) +
                          ',' +
                          IntToStr( PosRight.Col ) +
                          ')';
  LabelScore.Caption := IntToStr( ScoreLeft ) + ' - ' + IntToStr( ScoreRight );
end;

procedure TForm1.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  ClientSocket1.Close;
  ClientSocket1.Active := False;
  ClientSocket1.Destroy;
end;

procedure TForm1.FormShow(Sender: TObject);
begin
  Randomize;

  ClientSocket1 := TClientSocket.Create( Self );
  ClientSocket1.OnRead := OnRead;
  PlayerNamesReceived := False;

  PrevPosLeft.Row := 1;
  PrevPosLeft.Col := 1;
  PrevPosRight := PrevPosLeft;
  CurrentCycle := 0;
  PosLeft.Row := 4;
  PosLeft.Col := 2;
  PosRight.Row := 3;
  PosRight.Col := 8;
  ScoreLeft := 0;
  ScoreRight := 0; 

  RepaintField;
end;

procedure TForm1.Button2Click(Sender: TObject);
begin
  ClientSocket1.Port := StrToInt( EditPort.Text );
  ClientSocket1.Host := EditIP.Text;
  try
    ClientSocket1.Active := True;
  except
    ShowMessage( 'Cannot connect to the server' );
    Halt;
  end;

  button2.Enabled := False;
  Button3.Enabled := True;
end;

procedure TForm1.OnRead( Sender: TObject; Socket: TCustomWinSocket );
var
  TempStr : String;
  i : Integer;
begin
    ServerMsg := ClientSocket1.Socket.ReceiveText;
    if ServerMsg = '' then
      Exit;

    if (ServerMsg[ 1 ] = '_') AND (not PlayerNamesReceived) then
    begin
      for i := 2 to Length( ServerMsg ) do
        if ServerMsg[ i ] = '_' then
          Break;
      TempStr := Midstr( ServerMsg, 2, i - 2 );
      LabelLName.Caption := TempStr;
      TempStr := RightStr( ServerMsg, Length(ServerMsg) - i );
      LabelRName.Caption := TempStr;
      Exit;
    end;

    TempStr := LeftStr( ServerMsg, 1 );
    PosLeft.Row := StrToInt( TempStr );
    TempStr := MidStr( ServerMsg, 2, 1 );
    PosLeft.Col := StrToInt( TempStr );
    TempStr := MidStr( ServerMsg, 3, 1 );
    PosRight.Row := StrToInt( TempStr );
    TempStr := MidStr( ServerMsg, 4, 1 );
    PosRight.Col := StrToInt( TempStr );
    if ServerMsg[ 5 ] = 'P' then
      BallOwner := 'L'
    else if ServerMsg[ 5 ] = 'O' then
      BallOwner := 'R'
    else
      BallOwner := 'Unknown';
    TempStr := MidStr( ServerMsg, 7, 10 );
    CurrentCycle := StrToInt( TempStr );
    TempStr := MidStr( ServerMsg, 17, 5 );
    ScoreLeft := StrToInt( TempStr );
    TempStr := MidStr( ServerMsg, 22, 5 );
    ScoreRight := StrToInt( TempStr );
    LabelScore.Caption := IntToStr( ScoreLeft ) + ' - ' + IntToStr( ScoreRight );
    RepaintField;
end;


procedure TForm1.Button3Click(Sender: TObject);
begin
  if ClientSocket1.Active then
    ClientSocket1.Socket.SendText( 'I:__MONITOR__' )
  else
  begin
    ShowMessage( 'Monitor socket is not active' );
    Halt;
  end;
end;

end.
