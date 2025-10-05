// home-index.js

// Define main Angular module
var module = angular.module("homeIndex", ["homemovieEdit"]);

// Configure routes and hash prefix
module.config([
  "$routeProvider",
  "$locationProvider",
  function ($routeProvider, $locationProvider) {

    // ✅ Enable hashbang routing for IIS/ALB compatibility
    // Using '!' ensures routes like #!/movies work properly
    $locationProvider.html5Mode(false).hashPrefix("!");

    // ✅ Define application routes (relative paths only — no leading '/')
    $routeProvider
      .when("/", {
        controller: "HomeController",
        templateUrl: "templates/home.html"
      })
      .when("/movies", {
        controller: "HomeController",
        templateUrl: "templates/movies.html"
      })
      .when("/newMovie", {
        controller: "newMovieController",
        templateUrl: "templates/newMovie.html"
      })
      .when("/reviews/:Id", {
        controller: "reviewsController",
        templateUrl: "templates/reviews.html"
      })
      .otherwise({ redirectTo: "/" });
  }
]);

// ==================== Data Service ====================
module.factory("dataService", ["$http", "$q", function ($http, $q) {

  var _movies = [];
  var _reviews = [];
  var _isInit = false;
  var _singleReview = [];

  var _isReady = function () {
    return _isInit;
  };

  // Load all movies
  var _getMovies = function () {
    var deferred = $q.defer();
    $http.get("api/movies") // ✅ removed leading slash
      .then(function (result) {
        angular.copy(result.data, _movies);
        _isInit = true;
        deferred.resolve();
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Get a movie by Id
  var _getMovieById = function (Id) {
    var deferred = $q.defer();
    $http.get("api/movies/" + Id)
      .then(function (result) {
        deferred.resolve(result.data);
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Delete review
  var _removeReview = function (Id) {
    var deferred = $q.defer();
    $http.delete("api/MovieReviews/" + Id)
      .then(function () {
        deferred.resolve();
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Delete movie
  var _removeMovie = function (Id) {
    var deferred = $q.defer();
    $http.delete("api/Movies/" + Id)
      .then(function () {
        deferred.resolve();
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Add new movie
  var _addMovie = function (newMovie) {
    var deferred = $q.defer();
    $http.post("api/movies", newMovie)
      .then(function (result) {
        var newOne = result.data;
        _movies.splice(0, 0, newOne);
        deferred.resolve(newOne);
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Load reviews for movie
  var _getReviews = function (Id) {
    var deferred = $q.defer();
    $http.get("api/MovieReviews/" + Id)
      .then(function (result) {
        angular.copy(result.data, _reviews);
        _isInit = true;
        deferred.resolve();
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Lookup review by reviewer ID
  var _getReviewByReviewId = function (Id) {
    var deferred = $q.defer();
    $http.get("api/Lookups/getbyreviewerid?id=" + Id)
      .then(function (result) {
        var newOne = result.data;
        _isInit = true;
        deferred.resolve(newOne);
        toastr.success("Information retrieved");
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Add new review
  var _addReview = function (MovieId, newReview) {
    var deferred = $q.defer();
    $http.post("api/MovieReviews/" + MovieId, newReview)
      .then(function (result) {
        var newOne = result.data;
        _reviews.splice(0, 0, newOne);
        deferred.resolve();
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Edit review
  var _NewReviewEdit = function (newReview) {
    var deferred = $q.defer();
    $http.put("api/MovieReviews/", newReview)
      .then(function () {
        deferred.resolve();
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Edit movie
  var _movieEdit = function (Movie) {
    var deferred = $q.defer();
    $http.put("api/Movies/", Movie)
      .then(function () {
        deferred.resolve();
      }, function () {
        deferred.reject();
      });
    return deferred.promise;
  };

  // Get reviews by Id (cached)
  var _getReviewsById = function (Id) {
    var deferred = $q.defer();
    _getReviews(Id).then(function () {
      if (_reviews) deferred.resolve(_reviews);
      else deferred.reject();
    }, function () {
      deferred.reject();
    });
    return deferred.promise;
  };

  return {
    movies: _movies,
    reviews: _reviews,
    singleReview: _singleReview,
    getMovies: _getMovies,
    addMovie: _addMovie,
    isReady: _isReady,
    getReviews: _getReviews,
    addReview: _addReview,
    getReviewById: _getReviewsById,
    newReviewEdit: _NewReviewEdit,
    movieEdit: _movieEdit,
    getReviewByReviewId: _getReviewByReviewId,
    getMovieById: _getMovieById,
    removeReview: _removeReview,
    removeMovie: _removeMovie
  };
}]);

// ==================== Controllers ====================

// Home Controller
var HomeController = [
  "$scope", "dataService",
  function ($scope, dataService) {
    $scope.movies = 0;
    $scope.data = dataService;
    $scope.isBusy = false;

    if (!dataService.isReady()) {
      $scope.isBusy = true;
      dataService.getMovies()
        .then(function () {
          toastr.success("Data Retrieved Successfully");
        }, function () {
          toastr.error("Error Fetching Data");
        })
        .finally(function () {
          $scope.isBusy = false;
        });
    }
  }
];

// New Movie Controller
var newMovieController = [
  "$scope", "$window", "dataService",
  function ($scope, $window, dataService) {
    $scope.newMovie = {};
    $scope.save = function () {
      dataService.addMovie($scope.newMovie)
        .then(function () {
          toastr.success("Data Saved Successfully");
          $window.location = "#!/movies";
        }, function () {
          toastr.error("Couldn't Save the New Movie");
        });
    };
  }
];

// Reviews Controller
var reviewsController = [
  "$scope", "dataService", "$window", "$routeParams",
  function ($scope, dataService, $window, $routeParams) {
    $scope.review = null;
    $scope.MovieId = $routeParams.Id;
    $scope.newReview = {};

    dataService.getReviewById($routeParams.Id)
      .then(function (review) {
        $scope.review = review;
      }, function () {
        $window.location = "#!/";
      });

    $scope.saveReview = function () {
      dataService.addReview($scope.MovieId, $scope.newReview)
        .then(function () {
          toastr.success("Review Saved Successfully");
          $window.location = "#!/";
        }, function () {
          toastr.error("Couldn't Save the New Review");
        });
    };
  }
];