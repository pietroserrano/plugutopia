## Common

Progetto che contiene tutte le classi/interfacce utilizzate dai progetti che fungono da Gateway.

- IPlugin: interfaccia che rappresenta i vari plugin
- LocalPlugin: implementazione dei plugin, local in quanto vengono scaricati ed utilizzati localmente
- IPluginManager: interfaccia del gestore dei plugin
- PluginManager: classe che implementa l'interfaccia del gestore dei plugin, si occupa del caricamento dei plugin dinamico
- PluginLoadContext: classe che estende AssemblyLoadContext per il caricamento dinamico dei plugin
- 