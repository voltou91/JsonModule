# Introduction

Hello, 
I am currently studying computer science (gameplay programming but it's close enough) so my program is not perfect. However, I am open to any suggestions.

JsonModule allows you to search , read translations and data in a json file. The program recursively goes through a "Text" folder , maps languages ​​and additional datas which'll split into two modules. It handles replacements in strings and uses a third module to handle potential conditions in strings.
In addition, the program uses a cache to greatly improve the performance.

# Data Module

DataModule will take care of mapping everything that is not a language, like in the following example: 

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

DataModule will take care of "advices" mapping and ignore "fr" and "en" languages. The data can be of any type and organized to fit your coding style, you can convert the data into the type you want (while it's possible).

# Translation Module

On the other hand, Translation Module inherits from DataModule but will take care of mapping "fr" and "en" and ignore data like "advices". To separate languages from datas, we use an enum like this one:

```cs
public enum Languages{
    fr,
    en
}
```

You can add as many dialect as you wish here to tell the program to look for those languages in Json files.

# Operator Module

OperatorModule handles ternary conditions in strings and overrides. For Example with this string "Good {day?morning:evening} !", OperatorModule will get the ternary condition and replace the good result. If day (boolean here) is true, the string displays "Good morning !". Your conditions can be of type x>y?a:b, and supports nesting of ternary conditions such as x > y? x' == y'? a : b : c.

# Cache

To improve performance, the program uses "Cache" for all of the mentioned modules above.
You can preload the Cache of all Json of the program, which will have the effect of launching a missing key check in the translations of the same Json.
