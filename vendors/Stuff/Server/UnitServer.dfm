object Form1: TForm1
  Left = 187
  Top = 107
  ActiveControl = SpinWait
  BorderIcons = [biSystemMenu]
  BorderStyle = bsDialog
  Caption = 'Soccer Server'
  ClientHeight = 181
  ClientWidth = 292
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
  object Label1: TLabel
    Left = 3
    Top = 13
    Width = 27
    Height = 16
    Caption = 'Port:'
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -13
    Font.Name = 'MS Sans Serif'
    Font.Style = []
    ParentFont = False
  end
  object Label2: TLabel
    Left = 132
    Top = 16
    Width = 85
    Height = 13
    Caption = 'Wait Interval (ms):'
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = []
    ParentFont = False
  end
  object Label3: TLabel
    Left = 4
    Top = 45
    Width = 37
    Height = 13
    Caption = 'Log file:'
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'MS Sans Serif'
    Font.Style = []
    ParentFont = False
  end
  object Button1: TButton
    Left = 91
    Top = 71
    Width = 106
    Height = 25
    Caption = 'Initialize Server'
    TabOrder = 4
    OnClick = Button1Click
  end
  object Memo1: TMemo
    Left = 4
    Top = 104
    Width = 281
    Height = 73
    ReadOnly = True
    TabOrder = 5
  end
  object Edit1: TEdit
    Left = 44
    Top = 11
    Width = 57
    Height = 21
    TabOrder = 0
    Text = '55000'
  end
  object SpinWait: TSpinEdit
    Left = 228
    Top = 11
    Width = 57
    Height = 22
    Increment = 10
    MaxValue = 10000
    MinValue = 1
    TabOrder = 1
    Value = 1000
    OnChange = SpinWaitChange
  end
  object EditLogFile: TEdit
    Left = 44
    Top = 41
    Width = 165
    Height = 21
    TabOrder = 2
    Text = 'Log.txt'
  end
  object Button2: TButton
    Left = 216
    Top = 40
    Width = 69
    Height = 25
    Caption = 'Browse'
    TabOrder = 3
    OnClick = Button2Click
  end
  object Timer: TTimer
    Enabled = False
    Interval = 10
    OnTimer = TimerTimer
    Left = 4
    Top = 72
  end
  object SaveDialog: TSaveDialog
    Left = 32
    Top = 72
  end
end
