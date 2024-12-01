CREATE TABLE 
  NationsPerGame(
    gamename text not null,
    nationName text not null,
    primary key(
      gamename,
      nationName
    )
  );