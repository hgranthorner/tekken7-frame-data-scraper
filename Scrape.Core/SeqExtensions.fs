module Seq

let removeLast xs =
    Seq.take (Seq.length xs - 1) xs