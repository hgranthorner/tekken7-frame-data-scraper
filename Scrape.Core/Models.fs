module ScrapeFrameData.Models

open System

type TableString = TableString of string

type RowString = RowString of string

type Row =
    Row of {| Command: string
              HitLevel: string
              Damage: string
              StartUpFrame: string
              BlockFrame: string
              HitFrame: string
              CounterHitFrame: string
              Notes: string |}

type DbRow = DbRow of {| Id: string
                         CharacterName: string
                         Command: string
                         HitLevel: string
                         Damage: string
                         TotalDamage: int option
                         StartUpFrame: string
                         BlockFrame: string
                         HitFrame: string
                         CounterHitFrame: string
                         Notes: string |}

type Table = Table of Row list

type ParseTable = TableString -> Table

type ExtractTableStrings = string -> TableString list

type ExtractRowStrings = TableString -> RowString list

type ParseRow = RowString -> Row

type CombineRows = Row list -> Table
