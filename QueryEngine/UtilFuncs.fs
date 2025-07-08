namespace QueryEngine

open Models
open DataTypesEx

module UtilFuncs =

    // Matching Nulls
    let CheckForNull =
        function
        | null -> None
        | v -> Some(v)

    // Filter TokenPositionEx by Range
    let FilterTokenPositionExByRange (start: int) (end_: int) (tokenPositions: TokenPositionEx list) =
        tokenPositions
        |> List.filter (fun tp -> tp.DocumentID >= start && tp.DocumentID <= end_)
        |> List.sortBy (fun tp -> tp.DocumentID)

