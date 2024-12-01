CREATE TABLE 
  StrategyMapSetPerGame(
    gamename text not null,
    mapname text not null,
    maporder integer not null,
    primary key(
      gamename,
      mapname
    ),
    unique(
      gamename,
      mapname,
      maporder
    )
  );