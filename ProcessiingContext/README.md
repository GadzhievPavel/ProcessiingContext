# Перенос извещения об изменении между контекстами
## Назначение
Данная библиотека предназначена для работы с электронной структурой изделий в PLM системе **T-FLEX DOCs 17**.

Библиотека позволяет перемещать подготовленные изменения извещения об изменении (ИИ) между разными контекстами проектирования

Библиотека содержаит несколько инструментов для работы с ИИ
- Обертку для объектов ИИ [Notice](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/Model/Notice.cs)
[Примеры работы c Notice]() 
- Обработчик номенклатуры [NomenclatureHandler](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/Handler/NomenclatureHandler.cs)
- Обработчик файлов [FileHandler](https://github.com/GadzhievPavel/ProcessiingContext/blob/master/ProcessiingContext/Handler/FileHandler.cs)

Пример задачи решаемой библиотекой **ProcessingContext**

В ЭСИ существует сборка __3434343434 - ffjfjfj - B.1__. Сборка содержит в себе детали и сборочный чертеж

