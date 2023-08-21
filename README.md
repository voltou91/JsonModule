# Introduction

Hello, 
I am currently studying computer science so my program is not perfect. However, I am open to any suggestions.

JsonModule allows to search and read translations and data in a json file. The program recursively goes through a "Text" folder and maps languages ​​and additional data into two modules. It handles replacements in strings and uses a third module to handle potential conditions in strings.
In addition, the program uses a cache to greatly improve its performance.

# Data Module

DataModule will take care of mapping everything that is not a language, for example in the json file: 

```json
{
    "fr": {
        "name": "",
        "desc": "",
    },
    "en": {
        "name": "",
        "desc": "",
    },
    "advices": []
}
```

DataModule will take care of "advices" mapping and ignore "fr" and "en" languages. The data can be of any type and organized as you wish, you can convert the data into the type you want (provided it is possible).

# Translation Module

On the other side, Translation Module inherits from DataModule but will take care of mapping "fr" and "en" and ignore data like "advices". To separate languages from datas, we use an enum like :

```cs
public enum Languages{
    fr,
    en
}
```

You can add any language here to tell the program to look for that language in Json files.

# Operator Module

OperatorModule handles ternary conditions in strings and overrides. For Example with this string "Good {day?morning:evening} !", OperatorModule will get the ternary condition and replace the good result. If day (boolean here) is true, the string displays "Good morning !". Your conditions can be of type x>y?a:b, and supports nesting of ternary conditions such as x>y? x'==y'? a:b : c.

# Cache

To improve performance, the program uses "Cache" for all the modules mentioned above.
You can preload the Cache of all Json of the program, which will have the effect of launching a missing key check in the translations of the same Json.
