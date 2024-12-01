CREATE TABLE 
  ResourcePerNationsByGame(
    gamename text not null,
    nationName text not null,
    resourceName text not null,
    resourceAmount integer default 0,
    primary key(
      gamename,
      nationName,
      resourceName
    )
  );