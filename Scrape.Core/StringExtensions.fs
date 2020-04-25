module String

let indexOf (patternToFind: string) (str: string) =
    str.IndexOf patternToFind
    
let indexOfAfter (patternToFind: string) (index: int) (str: string) =
    str.IndexOf(patternToFind, index)

let lastIndexOf (patternToFind: string) (str: string) =
    str.LastIndexOf patternToFind
    
let split (pattern: string) (str: string) =
    str.Split pattern |> Array.toList
    
let substring startIndex length (str: string) =
    str.Substring(startIndex, length)

let substringFromPatterns (startPattern: string) (endPattern: string) (str: string) =
    let startIndex = str.IndexOf startPattern
    let endIndex = str.IndexOf endPattern
    let lengthOfPattern = String.length endPattern
    str.Substring(startIndex, endIndex - startIndex + lengthOfPattern)
    
let replace (oldValue: string) (newValue: string) (str: string) =
    str.Replace(oldValue, newValue)
