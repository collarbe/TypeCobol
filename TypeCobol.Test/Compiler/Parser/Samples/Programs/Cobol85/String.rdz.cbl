﻿000000 IDENTIFICATION DIVISION.
000000 PROGRAM-ID. StringStatement.
000000 ENVIRONMENT DIVISION.
000000 CONFIGURATION SECTION.
000000 SOURCE-COMPUTER. IBM-370.
       special-names. decimal-point is comma.
000000 DATA DIVISION.
000000 working-storage section.
000000 01 MyVar pic X.
000000 01 Result pic X(30).
000000
000000 PROCEDURE DIVISION.
000000     string MyVar ' ' MyVar
002620           delimited by size into Result
000000     .
000000 END PROGRAM MYPGM.