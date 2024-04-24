# Перенос извещения об изменении между контекстами
## Назначение
Данная библиотека предназначена для работы с электронной структурой изделий в PLM системе **T-FLEX DOCs 17**.

Библиотека позволяет параллельно работать с несколькими извещениями об изменениях, затрагивающими одинковые объекты.

Библиотека содержаит несколько инструментов для работы с ИИ
- Обертку для объектов ИИ [Notice](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/Model/Notice.cs)
[Примеры работы c Notice]() 
- Обработчик номенклатуры [NomenclatureHandler](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/Handler/NomenclatureHandler.cs)
- Обработчик файлов [FileHandler](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/Handler/FileHandler.cs)

## Пример задачи решаемой библиотекой **ProcessingContext**:


В ЭСИ существует сборка __3434343434 - ffjfjfj - B.1__. Сборка содержит в себе детали и сборочный чертеж

![logo](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/docs/img/sourceAssembly.PNG)

Подготавливаем изменение состава сборки в контексте Изменения

![img](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/docs/img/sourceAssembly.PNG)

Создаем извещение об изменении. У изменения объект соответствие подключений выглядит следующим образом

![img1](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/docs/img/match.PNG)

Отправляем на согласование ИИ и ожидаем принятия изменений.

Если бизнес-логика процесса продолжительная, то может возникнуть необходимость выпустить еще одно ИИ. Если изменение в котексте имеет статус (Удаление или Добавление и т.п.), то сформировать ИИ не возможно. Также изменение подключений, свяызанных с согласуемым в текущий момент ИИ может привести к ошибки при применении изменений

Для решения этой проблемы создаем дополнительный контекст проектирования.

Данная библиотека позволяет перемещать ИИ со всеми связанными объектами между контекстами, заданными пользователями.



## Функции библиотеки **ProcessingContext**

#### Перенос в заданный контекст

Метод __MoveToContext__ позволяет перенести ИИ в указанный контекст

Пример кода
<pre>
<code>
        var ii = Context.ReferenceObject;
        var currentConfig = Notice.GetConfigModifications(ii, Context.Connection);
        var notice = new Notice(ii, Context.Connection, currentConfig);
        notice.MoveToContext(designContext);
</code>
</pre>

#### Проверка возможности переноса в заданный контекст

Метод __IsEnableMoveInContext__ возвращает true, если ИИ можно перенести в заданный контекст. Критерием проверки является наличие в целевом контексте исходного подключения

<pre>
<code>
    public bool isEnableReturnContext()
    {
        ReferenceObject ii = (ReferenceObject)Объект;
        var config = Notice.GetConfigModifications(ii, Context.Connection);
        Notice notice = new Notice(ii, Context.Connection, config);

        DesignContextsReference designContextsReference = new DesignContextsReference(Context.Connection);
        var designContext = designContextsReference.Find("Изменения") as DesignContextObject;

        return notice.IsEnableMoveInContext(designContext);
    }
</code>
</pre>

![logo](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/docs/img/A4%20-%20107.jpg)