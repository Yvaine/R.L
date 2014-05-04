object SimpleClient: TSimpleClient
  Left = 258
  Top = 17
  BorderIcons = [biSystemMenu]
  BorderStyle = bsDialog
  Caption = 'Simple Client'
  ClientHeight = 101
  ClientWidth = 272
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object Bevel1: TBevel
    Left = 8
    Top = 104
    Width = 257
    Height = 161
  end
  object Label1: TLabel
    Left = 16
    Top = 46
    Width = 56
    Height = 13
    Caption = 'Server Port:'
  end
  object Label2: TLabel
    Left = 16
    Top = 70
    Width = 63
    Height = 13
    Caption = 'Player Name:'
  end
  object Label3: TLabel
    Left = 16
    Top = 22
    Width = 47
    Height = 13
    Caption = 'Server IP:'
  end
  object Label4: TLabel
    Left = 16
    Top = 120
    Width = 57
    Height = 13
    Caption = 'My Position:'
  end
  object Label5: TLabel
    Left = 16
    Top = 144
    Width = 90
    Height = 13
    Caption = 'Opponent Position:'
  end
  object Label6: TLabel
    Left = 16
    Top = 168
    Width = 31
    Height = 13
    Caption = 'Score:'
  end
  object Label7: TLabel
    Left = 16
    Top = 192
    Width = 54
    Height = 13
    Caption = 'Ball Owner:'
  end
  object Label8: TLabel
    Left = 16
    Top = 216
    Width = 66
    Height = 13
    Caption = 'Current Cycle:'
  end
  object LabelMyPos: TLabel
    Left = 112
    Top = 120
    Width = 58
    Height = 13
    Caption = 'LabelMyPos'
  end
  object LabelOppPos: TLabel
    Left = 112
    Top = 144
    Width = 58
    Height = 13
    Caption = 'LabelMyPos'
  end
  object LabelScore: TLabel
    Left = 112
    Top = 168
    Width = 54
    Height = 13
    Caption = 'LabelScore'
  end
  object LabelBallOwner: TLabel
    Left = 112
    Top = 192
    Width = 74
    Height = 13
    Caption = 'LabelBallOwner'
  end
  object LabelCurrentCycle: TLabel
    Left = 112
    Top = 216
    Width = 86
    Height = 13
    Caption = 'LabelCurrentCycle'
  end
  object Label9: TLabel
    Left = 16
    Top = 240
    Width = 59
    Height = 13
    Caption = 'Game State:'
  end
  object LabelGameState: TLabel
    Left = 112
    Top = 240
    Width = 79
    Height = 13
    Caption = 'LabelGameState'
  end
  object Bevel2: TBevel
    Left = 8
    Top = 8
    Width = 257
    Height = 89
  end
  object Button2: TButton
    Left = 184
    Top = 39
    Width = 73
    Height = 25
    Caption = 'Initialize'
    TabOrder = 0
    OnClick = Button2Click
  end
  object EditPort: TEdit
    Left = 88
    Top = 42
    Width = 89
    Height = 21
    TabOrder = 1
    Text = '55000'
  end
  object EditName: TEdit
    Left = 88
    Top = 66
    Width = 89
    Height = 21
    TabOrder = 2
    Text = 'Player'
  end
  object EditIP: TEdit
    Left = 88
    Top = 18
    Width = 89
    Height = 21
    TabOrder = 3
    Text = '127.0.0.1'
  end
  object Button1: TButton
    Left = 184
    Top = 64
    Width = 73
    Height = 25
    Caption = 'Send Init Info'
    Enabled = False
    TabOrder = 4
    OnClick = Button1Click
  end
end
