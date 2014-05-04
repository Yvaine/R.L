unit UnitLogPlayer;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, ExtCtrls, StrUtils, Spin, MPlayer, Buttons;

type TPosition = record
  Row : Integer;
  Col : Integer;
end;

type
  TFormLogPlayer = class(TForm)
    ImField: TImage;
    ImBlank: TImage;
    ImLeftBall: TImage;
    ImLeft: TImage;
    ImRightBall: TImage;
    ImRight: TImage;
    Panel1: TPanel;
    Label1: TLabel;
    LabelGameState: TLabel;
    Bevel1: TBevel;
    Label3: TLabel;
    Label4: TLabel;
    LabelLName: TLabel;
    LabelLeftPos: TLabel;
    LabelRightPos: TLabel;
    LabelScore: TLabel;
    LabelRName: TLabel;
    Bevel2: TBevel;
    Label7: TLabel;
    Label8: TLabel;
    Label5: TLabel;
    EditLogFile: TEdit;
    Button1: TButton;
    OpenDialog: TOpenDialog;
    Label6: TLabel;
    SpinDelay: TSpinEdit;
    Label9: TLabel;
    SpinCycle: TSpinEdit;
    Button2: TButton;
    Timer: TTimer;
    BitBtn1: TBitBtn;
    BitBtn2: TBitBtn;
    BitBtn3: TBitBtn;
    BitBtn4: TBitBtn;
    BitBtn5: TBitBtn;
    Label2: TLabel;
    LabelBallOwner: TLabel;
    procedure FormShow(Sender: TObject);
    procedure Button1Click(Sender: TObject);
    procedure Button2Click(Sender: TObject);
    procedure SpinDelayChange(Sender: TObject);
    procedure TimerTimer(Sender: TObject);
    procedure BitBtn1Click(Sender: TObject);
    procedure BitBtn2Click(Sender: TObject);
    procedure BitBtn3Click(Sender: TObject);
    procedure BitBtn4Click(Sender: TObject);
    procedure BitBtn5Click(Sender: TObject);
  private
    PrevPosLeft, PrevPosRight : TPosition;
    PosLeft, PosRight : TPosition;
    ScoreLeft, ScoreRight : Cardinal;
    CurrentCycle : Cardinal;
    TotalCycles : Cardinal;
    BallOwner : String;
    GameState : Char;
    CanUseButtons : Boolean;
    PlayerNamesReceived : Boolean;
    MsgArray : array of String;
    Origin : TPoint;

    procedure ParseMessage( Msg : String );
    procedure RepaintField;

  public
    { Public declarations }
  end;

var
  FormLogPlayer: TFormLogPlayer;

implementation

uses Math;

{$R *.dfm}

procedure TFormLogPlayer.RepaintField;
begin
  ImField.Canvas.Draw( (PrevPosLeft.Col - 1)* 23 + Origin.X, (PrevPosLeft.Row - 1)* 23 + Origin.Y, ImBlank.Picture.Bitmap );
  ImField.Canvas.Draw( (PrevPosRight.Col - 1)* 23 + Origin.X, (PrevPosRight.Row - 1)* 23 + Origin.Y, ImBlank.Picture.Bitmap );

  if BallOwner = 'Left' then
  begin
    ImField.Canvas.Draw( (PosLeft.Col - 1)*23 + Origin.X, (PosLeft.Row - 1)*23 + Origin.Y, ImLeftBall.Picture.Bitmap );
    ImField.Canvas.Draw( (PosRight.Col - 1)*23 + Origin.X, (PosRight.Row - 1)*23 + Origin.Y, ImRight.Picture.Bitmap );
    LabelBallOwner.Caption := 'Left';
  end
  else if BallOwner = 'Right' then
  begin
    ImField.Canvas.Draw( (PosLeft.Col - 1)*23 + Origin.X, (PosLeft.Row - 1)*23 + Origin.Y, ImLeft.Picture.Bitmap );
    ImField.Canvas.Draw( (PosRight.Col - 1)*23 + Origin.X, (PosRight.Row - 1)*23 + Origin.Y, ImRightBall.Picture.Bitmap );
    LabelBallOwner.Caption := 'Right';
  end
  else
  begin
    ImField.Canvas.Draw( (PosLeft.Col - 1)*23 + Origin.X, (PosLeft.Row - 1)*23 + Origin.Y, ImLeft.Picture.Bitmap );
    ImField.Canvas.Draw( (PosRight.Col - 1)*23 + Origin.X, (PosRight.Row - 1)*23 + Origin.Y, ImRight.Picture.Bitmap );
    LabelBallOwner.Caption := 'Unknown';
  end;

  PrevPosLeft := PosLeft;
  PrevPosRight := PosRight;

  if PlayerNamesReceived then
  begin
  end;
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
  case GameState of
    'C': LabelGameState.Caption := 'Play On';
    'L': LabelGameState.Caption := 'Left Goal';
    'R': LabelGameState.Caption := 'Right Goal';
    'F': LabelGameState.Caption := 'Finished';
  end;

end;

procedure TFormLogPlayer.FormShow(Sender: TObject);
begin
  Randomize;

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
  BallOwner := 'Unknown';
  CanUseButtons := False;

  Origin.X := 25;
  Origin.Y := 2;
  RepaintField;
end;

procedure TFormLogPlayer.Button1Click(Sender: TObject);
begin
  if OpenDialog.Execute then
  begin
    EditLogFile.Text := OpenDialog.FileName;
    if EditLogFile.Text <> '' then
      Button2.Enabled := True;
  end;
end;

procedure TFormLogPlayer.Button2Click(Sender: TObject);
var
  FHandle : TextFile;
  ReadStr : String;
  i : Cardinal;
begin
  try
    AssignFile( Fhandle, EditLogFile.Text );
    Reset( FHandle );
  except
    ShowMessage( 'Cannot open file' );
    Exit;
  end;

  Button2.Enabled := False;
  ReadLn( FHandle, ReadStr );
  LabelLName.Caption := ReadStr;
  ReadLn( FHandle, ReadStr );
  LabelRName.Caption := ReadStr;

  SetLength( MsgArray, 10000 + 1 );
  i := 1;
  while not EOF( FHandle ) do
  begin
    if i >= Length( MsgArray ) then
      SetLength( MsgArray, Length( MsgArray ) + 10000 );
    Readln( FHandle, MsgArray[ i ] );
    i := i + 1;
  end;
  MsgArray[ 0 ] := '4238' + MsgArray[ 1, 5 ] + 'C00000000000000000000';
  TotalCycles := i;
  SetLength( MsgArray, TotalCycles ); 
  CloseFile( FHandle );
  SpinCycle.Enabled := True;
  Button2.Enabled := True;
  SpinCycle.Value := 0;

  ParseMessage( MsgArray[ 0 ] );
  RepaintField;

  CanUseButtons := True;
  BitBtn1.Visible := True;
  BitBtn2.Visible := True;
  BitBtn3.Visible := True;
  BitBtn4.Visible := True;
  BitBtn5.Visible := True;


  timer.Enabled := False;
  timer.Interval := SpinDelay.Value;
end;

procedure TFormLogPlayer.ParseMessage( Msg : String );
var
  TempStr : String;
begin
    TempStr := LeftStr( Msg, 1 );
    PosLeft.Row := StrToInt( TempStr );
    TempStr := MidStr( Msg, 2, 1 );
    PosLeft.Col := StrToInt( TempStr );
    TempStr := MidStr( Msg, 3, 1 );
    PosRight.Row := StrToInt( TempStr );
    TempStr := MidStr( Msg, 4, 1 );
    PosRight.Col := StrToInt( TempStr );
    if Msg[ 5 ] = 'P' then
      BallOwner := 'Left'
    else if Msg[ 5 ] = 'O' then
      BallOwner := 'Right'
    else
      BallOwner := 'Unknown';
    GameState := Msg[ 6 ];
    //TempStr := MidStr( Msg, 7, 10 );
    //CurrentCycle := StrToInt( TempStr );
    TempStr := MidStr( Msg, 17, 5 );
    ScoreLeft := StrToInt( TempStr );
    TempStr := MidStr( Msg, 22, 5 );
    ScoreRight := StrToInt( TempStr );
end;

procedure TFormLogPlayer.SpinDelayChange(Sender: TObject);
begin
  Timer.Interval := SpinDelay.Value;
end;

procedure TFormLogPlayer.TimerTimer(Sender: TObject);
begin
  SpinCycle.Value := (SpinCycle.Value + 1) mod TotalCycles;
  ParseMessage( MsgArray[ SpinCycle.Value ] );
  RepaintField;
end;

procedure TFormLogPlayer.BitBtn1Click(Sender: TObject);
begin
  if not CanUseButtons then
    Exit;

  Timer.Enabled := False;
  SpinCycle.Value := SpinCycle.Value - 1;
  if SpinCycle.Value < 0 then
    SpinCycle.Value := SpinCycle.Value + TotalCycles;
  ParseMessage( MsgArray[ SpinCycle.Value ] );
  RepaintField;
end;

procedure TFormLogPlayer.BitBtn2Click(Sender: TObject);
begin
  if not CanUseButtons then
    Exit;

  Timer.Interval := SpinDelay.Value;
  Timer.Enabled := True;
end;

procedure TFormLogPlayer.BitBtn3Click(Sender: TObject);
begin
  if not CanUseButtons then
    Exit;

  Timer.Enabled := False;
end;

procedure TFormLogPlayer.BitBtn4Click(Sender: TObject);
begin
  if not CanUseButtons then
    Exit;

  Timer.Enabled := False;
  SpinCycle.Value := 0;
  ParseMessage( MsgArray[ 0 ] );
  RepaintField;
end;

procedure TFormLogPlayer.BitBtn5Click(Sender: TObject);
begin
  if not CanUseButtons then
    Exit;

  Timer.Enabled := False;
  SpinCycle.Value := (SpinCycle.Value + 1) mod TotalCycles;
  ParseMessage( MsgArray[ SpinCycle.Value ] );
  RepaintField;
end;

end.
