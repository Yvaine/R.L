unit UnitSServer;

interface

uses Math;

const FIELD_LENGTH = 9;      //length of the the soccer field
const FIELD_WIDTH  = 6;      //width of the soccer field
const GAME_LENGTH  = 10000; //Number of game cycles

type TPosition = record
  Row : Integer;
  Col : Integer;
end;

type TCommand = (Hold, North, South, West, East);

type TSide = (SLeft, SRight);

type TPlayer = record
  Position      : TPosition;
  Score         : Cardinal;
  LastCommand   : TCommand;
  Side          : TSide;
  HaveBall      : Boolean;
  Name          : String;
end;

type TGameState = ( GSPlayOn, //The Game is running
                    GSGoalLeft, //Left player scores
                    GSGoalRight, //Right player scores
                    GSFinished   // End of the game
                  );

type
TSServer = class
public
  constructor Create( APlayerNameLeft, APlayerNameRight : String );
  destructor  Destroy; override;

  procedure SetCommandPlayerLeft( AComm : TCommand );
  procedure SetCommandPlayerRight( AComm : TCommand );
  procedure PerformCommands;
  function  GetPositionPlayerLeft : TPosition;
  function  GetPositionPlayerRight : TPosition;
  function  GetBallOwnerSide : TSide;
  function  GetScorePlayerLeft : Cardinal;
  function  GetScorePlayerRight: Cardinal;
  function  GetCurrentCycle : Cardinal;
  function  GetGameState : TGameState;
  function  GetLeftPlayerName : String;
  function  GetRightPlayerName : String;
  function  InversePos( APos : TPosition ): TPosition;
  function  GetRandomCommand: TCommand;

private
  LeftPlayer  : TPlayer;
  RightPlayer : TPlayer;
  TimeCurrent : Cardinal;
  GameState   : TGameState;

  InitPosLeft : TPosition;

  function  InverseCommand( AComm : TCommand ): TCommand;
  function  IsInGoalLeft( APos : TPosition ): Boolean;
  function  IsInGoalRight( APos : TPosition ): Boolean;
  function  IsEqualPos( APos1, APos2 : TPosition ): Boolean;
  procedure SetRandomBallOwner;
  procedure CheckForGoal;
  procedure MakePositionsConsistent;


end; //end of class declaration

implementation

constructor TSServer.Create( APlayerNameLeft, APlayerNameRight : String );
begin
  Randomize;

  InitPosLeft.Row := 4;
  InitPosLeft.Col := 2;

  LeftPlayer.Position := InitPosLeft;
  RightPlayer.Position := InversePos( InitPosLeft );

  LeftPlayer.Score := 0;
  RightPlayer.Score := 0;

  LeftPlayer.Side := SLeft;
  RightPlayer.Side := SRight;

  LeftPlayer.LastCommand := Hold;
  RightPlayer.LastCommand := Hold;

  LeftPlayer.Name := APlayerNameLeft;
  RightPlayer.Name := APlayerNameRight;

  SetRandomBallOwner;

  TimeCurrent := 0;

  GameState := GSPlayOn;
end; //end of constrcutor

destructor TSServer.Destroy;
begin
  //Nothing
end; //end of destructor

procedure TSServer.SetCommandPlayerLeft( AComm : TCommand );
begin
  LeftPlayer.LastCommand := AComm;
end;

procedure TSServer.SetCommandPlayerRight( AComm : TCommand );
begin
  RightPlayer.LastCommand := AComm;
end;

procedure TSServer.PerformCommands;
var
  PrevPosLeft, PrevPosRight : TPosition;
  IsLeftMoved, IsRightMoved : Boolean;
begin
  if TimeCurrent >= GAME_LENGTH then
  begin
    GameState := GSFinished;
    Exit;
  end;

  Inc( TimeCurrent );
  GameState := GSPlayOn;
  RightPlayer.LastCommand := InverseCommand( RightPlayer.LastCommand );

  PrevPosLeft := LeftPlayer.Position;
  PrevPosRight := RightPlayer.Position;

  IsLeftMoved := True;
  IsRightMoved := True;
  if LeftPlayer.LastCommand = Hold then
    IsLeftMoved := False;
  if RightPlayer.LastCommand = Hold then
    IsRightMoved := False;

  if Random < 0.5 then //Left player's action should be performed first
  begin
    case LeftPlayer.LastCommand of
      North: Dec( LeftPlayer.Position.Row );
      South: Inc( LeftPlayer.Position.Row );
      West:  Dec( LeftPlayer.Position.Col );
      East:  Inc( LeftPlayer.Position.Col );
    end; //end of case
    case RightPlayer.LastCommand of
      North: Dec( RightPlayer.Position.Row );
      South: Inc( RightPlayer.Position.Row );
      West:  Dec( RightPlayer.Position.Col );
      East:  Inc( RightPlayer.Position.Col );
    end; //end of case
    CheckForGoal;
    MakePositionsConsistent;
    if IsEqualPos( LeftPlayer.Position, RightPlayer.Position ) then
    begin
      if IsRightMoved then
      begin
        LeftPlayer.HaveBall := True;
        RightPlayer.HaveBall := False;
        RightPlayer.Position := PrevPosRight;
      end
      else
      begin
        LeftPlayer.HaveBall := False;
        RightPlayer.HaveBall := True;
        LeftPlayer.Position := PrevPosLeft;
      end;
    end;
  end
  else //Right player's action should be performed first
  begin
    case RightPlayer.LastCommand of
      North: Dec( RightPlayer.Position.Row );
      South: Inc( RightPlayer.Position.Row );
      West:  Dec( RightPlayer.Position.Col );
      East:  Inc( RightPlayer.Position.Col );
    end; //end of case
    case LeftPlayer.LastCommand of
      North: Dec( LeftPlayer.Position.Row );
      South: Inc( LeftPlayer.Position.Row );
      West:  Dec( LeftPlayer.Position.Col );
      East:  Inc( LeftPlayer.Position.Col );
    end; //end of case
    CheckForGoal;
    MakePositionsConsistent;
    if IsEqualPos( LeftPlayer.Position, RightPlayer.Position ) then
    begin
      if IsLeftMoved then
      begin
        RightPlayer.HaveBall := True;
        LeftPlayer.HaveBall := False;
        LeftPlayer.Position := PrevPosLeft;
      end
      else
      begin
        RightPlayer.HaveBall := False;
        LeftPlayer.HaveBall := True;
        RightPlayer.Position := PrevPosRight;
      end;
    end;
  end;
end;

function  TSServer.GetPositionPlayerLeft : TPosition;
begin
  Result := LeftPlayer.Position;
end;

function  TSServer.GetPositionPlayerRight : TPosition;
begin
  Result := RightPlayer.Position;
end;

function  TSServer.GetBallOwnerSide : TSide;
begin
  if LeftPlayer.HaveBall then
    Result := SLeft
  else
    Result := SRight;
end;

function  TSServer.GetScorePlayerLeft : Cardinal;
begin
  Result := LeftPlayer.Score;
end;

function  TSServer.GetScorePlayerRight: Cardinal;
begin
  Result := RightPlayer.Score;
end;

function  TSServer.GetGameState : TGameState;
begin
  Result := GameState;
end;

function  TSServer.InversePos( APos : TPosition ): TPosition;
begin
  Result.Row := FIELD_WIDTH + 1 - Apos.Row;
  Result.Col := FIELD_LENGTH + 1 - APos.Col;
end;

function  TSServer.InverseCommand( AComm : TCommand ) : TCommand;
begin
  Result := AComm;
  case AComm of
    North : Result := South;
    South : Result := North;
    West  : Result := East;
    East  : Result := West;
  end;
end;

function TSServer.IsInGoalLeft( APos : TPosition ): Boolean;
begin
  Result := False;
  if (APos.Col = 0) AND ((APos.Row = 3) OR (APos.Row = 4)) then
    Result := True;
end;

function TSServer.IsInGoalRight( APos : TPosition ): Boolean;
begin
  APos := InversePos( APos );
  Result := IsInGoalLeft( APos );
end;

procedure TSServer.SetRandomBallOwner;
begin
  LeftPlayer.HaveBall := False;
  RightPlayer.HaveBall := False;
  if Random < 0.5 then
    LeftPlayer.HaveBall := True
  else
    RightPlayer.HaveBall := True;
end;

procedure TSServer.CheckForGoal;
begin
  if ( LeftPlayer.HaveBall AND IsInGoalRight(LeftPlayer.Position) ) OR
     ( RightPlayer.HaveBall AND IsInGoalRight(RightPlayer.Position) ) then //Goal Left
  begin
    GameState := GSGoalLeft;
    Inc( LeftPlayer.Score );
    LeftPlayer.Position := InitPosLeft;
    RightPlayer.Position := InversePos( InitPosLeft );
    SetRandomBallOwner;
  end
  else if ( RightPlayer.HaveBall AND IsInGoalLeft(RightPlayer.Position) ) OR
     ( LeftPlayer.HaveBall AND IsInGoalLeft(LeftPlayer.Position) ) then //Goal Right
  begin
    GameState := GSGoalRight;
    Inc( RightPlayer.Score );
    LeftPlayer.Position := InitPosLeft;
    RightPlayer.Position := InversePos( InitPosLeft );
    SetRandomBallOwner;
  end;
end;

procedure TSServer.MakePositionsConsistent;
begin
  with LeftPlayer.Position do
  begin
    if Row < 1 then
      Row := 1
    else if Row > FIELD_WIDTH then
      Row := FIELD_WIDTH;

    if Col < 1 then
      Col := 1
    else if Col > FIELD_LENGTH then
      Col := FIELD_LENGTH;
  end;

  with RightPlayer.Position do
  begin
    if Row < 1 then
      Row := 1
    else if Row > FIELD_WIDTH then
      Row := FIELD_WIDTH;

    if Col < 1 then
      Col := 1
    else if Col > FIELD_LENGTH then
      Col := FIELD_LENGTH;
  end;
end;

function TSServer.IsEqualPos( APos1, APos2 : TPosition ): Boolean;
begin
  Result := False;
  if (APos1.Row = APos2.Row) AND (APos1.Col = APos2.Col) then
    Result := True;
end;

function TSServer.GetCurrentCycle : Cardinal;
begin
  Result := TimeCurrent;
end;

function TSServer.GetLeftPlayerName : String;
begin
  Result := LeftPlayer.Name;
end;

function TSServer.GetRightPlayerName : String;
begin
  Result := RightPlayer.Name;
end;

function TSServer.GetRandomCommand: TCommand;
var
  RandNum : Real;
  RandInt : Integer;
begin
  Result := Hold;
  RandNum := Random;
  RandInt := Floor( RandNum * 5 );
  case RandInt of
    0: Result := Hold;
    1: Result := North;
    2: Result := South;
    3: Result := West;
    4: Result := East;
  end;

end;

end.
