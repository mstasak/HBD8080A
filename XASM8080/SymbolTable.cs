using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XASM8080;
public class SymbolTable {

    private static readonly Lazy<SymbolTable> lazy =
        new(() => new SymbolTable());

    public static SymbolTable Instance => lazy.Value;

    private SymbolTable() {
        SymbolTab = new();
        BlockTab = new();
        BlockStacks = new();

    }


    public Dictionary<string, SymbolDefinition> SymbolTab; //complex globbed key [$=global]label[.sublabel]@file or blockname:label@filename
    public Dictionary<string, BlockDefinition> BlockTab; //key=globbed filename.blockname
    public Dictionary<string, Stack<string>> BlockStacks; //key=filename, stack = list of unique nestable blocknames

    public SymbolDefinition? Lookup(string name) {
        return SymbolTab[name];
    }

    public SymbolDefinition Add(SymbolDefinition sym) {
        SymbolDefinition? rslt = SymbolTab[sym.Name] as SymbolDefinition;
        if (SymbolTab.ContainsKey(sym)) {

        } else {
            SymbolTab.Add(sym.Name, sym);
        }
        return rslt;
    }

    internal int UnresolvedSymbolCount() {
        throw new NotImplementedException();
    }

    internal int UnresolvedSymbolRefCount() {
        throw new NotImplementedException();
    }
    
}
