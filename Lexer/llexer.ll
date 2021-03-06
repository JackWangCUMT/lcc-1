﻿
%N $[A-Z_]+$

%%

using Lexer;

%%

$\$(\\[^\n\r\t]|[^\n\r\t\$\\])*\$$  
    tokens.Add(new T_REGEX(text.Substring(1, text.Length - 2)));

$[^\$ \n\r\t%][^\r\n]*$
    tokens.Add(new T_CODE(text));

$%{N}$
    tokens.Add(new T_ALIAS(text.Substring(1)));

$[ \n\r\t]+$
    
$%%$
    tokens.Add(new T_SPLITER());
%%
