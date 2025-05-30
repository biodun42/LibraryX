// User Library JavaScript functionality
document.addEventListener("DOMContentLoaded", function () {
  // Tab switching functionality
  setupTabSwitching();

  // Description toggle functionality
  setupDescriptionToggles();

  // Sort functionality for books
  setupBookSorting();

  // Delete download functionality with loading animation
  setupDeleteFunctionality();

  // Subscription management functionality
  setupSubscriptionManagement();

  // Book search functionality
  setupBookSearch();
});

// Tab switching functionality
function setupTabSwitching() {
  const tabButtons = document.querySelectorAll(".tab-button");
  const tabContents = document.querySelectorAll(".tab-content");
  const tabIndicator = document.querySelector(".tab-indicator");

  // Listen for hash changes in the URL
  window.addEventListener("hashchange", function () {
    const hash = window.location.hash.substring(1);
    if (hash && document.getElementById(hash)) {
      setActiveTab(hash);
    }
  });

  function setActiveTab(target) {
    // Reset all tabs
    tabButtons.forEach((btn) => {
      btn.classList.remove("active");
      btn.classList.remove("bg-primary-900", "text-white");
      btn.classList.add(
        "bg-white",
        "text-gray-700",
        "border",
        "border-gray-200"
      );
    });

    // Hide all tab contents
    tabContents.forEach((content) => {
      content.classList.add("hidden");
    });

    // Set active tab
    const selectedBtn = document.querySelector(
      `.tab-button[data-target="${target}"]`
    );
    const selectedContent = document.getElementById(target);

    if (selectedBtn && selectedContent) {
      selectedBtn.classList.add("active");
      selectedBtn.classList.add("bg-primary-900", "text-white");
      selectedBtn.classList.remove(
        "bg-white",
        "text-gray-700",
        "border",
        "border-gray-200"
      );

      // Update indicator position
      if (tabIndicator) {
        tabIndicator.style.width = `${selectedBtn.offsetWidth}px`;
        tabIndicator.style.left = `${selectedBtn.offsetLeft}px`;
      }

      // Animate content entrance
      selectedContent.classList.remove("hidden");
      selectedContent.classList.add("animate-fade-in");

      // Refresh description toggles when tab changes
      setupDescriptionToggles();
    }
  }

  tabButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const target = button.dataset.target;
      setActiveTab(target);
    });
  });
  // Initialize active tab - check URL hash first, then for active class, then default to first tab
  if (tabButtons.length > 0) {
    let initialTab = tabButtons[0].dataset.target;

    // Check URL hash
    const urlHash = window.location.hash.substring(1);
    if (urlHash && document.getElementById(urlHash)) {
      initialTab = urlHash;
    } else if (document.querySelector(".tab-button.active")) {
      initialTab = document.querySelector(".tab-button.active").dataset.target;
    }

    setActiveTab(initialTab);
  }
}

// Book description toggle functionality
function setupDescriptionToggles() {
  const toggleBtns = document.querySelectorAll(".toggle-description");

  toggleBtns.forEach((btn) => {
    // Make sure we're not attaching duplicate event listeners
    btn.removeEventListener("click", toggleDescriptionHandler);
    btn.addEventListener("click", toggleDescriptionHandler);
  });

  function toggleDescriptionHandler() {
    const container = this.closest(".description-container");
    const description = container.querySelector(".book-description");
    const fadeEl = container.querySelector(".description-fade");
    const icon = this.querySelector(".toggle-icon");
    const textSpan = this.querySelector("span");

    if (description) {
      if (description.classList.contains("expanded")) {
        // Collapse
        description.classList.remove("expanded");
        icon.style.transform = "rotate(0deg)";
        textSpan.textContent = "Read more";
        fadeEl.style.display = "block";
      } else {
        // Expand
        description.classList.add("expanded");
        icon.style.transform = "rotate(180deg)";
        textSpan.textContent = "Read less";
        fadeEl.style.display = "none";
      }
    }
  }
}

// Setup book sorting functionality
function setupBookSorting() {
  const sortSelect = document.getElementById("sortBooks");
  const booksContainer = document.getElementById("booksListView");

  if (sortSelect && booksContainer) {
    sortSelect.addEventListener("change", function () {
      const books = Array.from(booksContainer.children);
      const sortValue = this.value;

      books.sort((a, b) => {
        const titleA = a.querySelector("h3").textContent;
        const titleB = b.querySelector("h3").textContent;

        if (sortValue === "title_asc") {
          return titleA.localeCompare(titleB);
        } else if (sortValue === "title_desc") {
          return titleB.localeCompare(titleA);
        } else if (sortValue === "date_asc" || sortValue === "date_desc") {
          // Extract date from the book element
          const dateElements = {
            a: a.querySelector(".book-date"),
            b: b.querySelector(".book-date"),
          };

          const dateA = dateElements.a
            ? new Date(dateElements.a.dataset.date)
            : new Date(0);
          const dateB = dateElements.b
            ? new Date(dateElements.b.dataset.date)
            : new Date(0);

          return sortValue === "date_asc" ? dateA - dateB : dateB - dateA;
        }

        return 0;
      });

      books.forEach((book) => {
        booksContainer.appendChild(book);
      });
    });
  }
}

// Setup delete functionality with loading animation
function setupDeleteFunctionality() {
  document.querySelectorAll(".delete-download-btn").forEach((btn) => {
    btn.addEventListener("click", async function (e) {
      e.preventDefault();
      const downloadId = this.dataset.id;
      const downloadTitle = this.dataset.title;
      const downloadItem = this.closest(".download-item");

      // Create loading animation in the button
      const originalContent = this.innerHTML;
      this.innerHTML =
        '<i class="fas fa-spinner fa-spin mr-1.5"></i> <span class="hidden sm:inline">Deleting...</span>';
      this.disabled = true;

      // Add a faded overlay effect to the item
      downloadItem.style.opacity = "0.7";
      downloadItem.style.pointerEvents = "none";

      try {
        // Create form with anti-forgery token
        const form = document.createElement("form");
        form.method = "POST";
        form.action = "/UserLibrary/RemoveDownloaded";
        form.style.display = "none";

        // Add the download ID
        const idInput = document.createElement("input");
        idInput.type = "hidden";
        idInput.name = "id";
        idInput.value = downloadId;

        // Clone the anti-forgery token
        const antiForgeryToken = document.querySelector(
          'input[name="__RequestVerificationToken"]'
        );
        if (antiForgeryToken) {
          const tokenInput = antiForgeryToken.cloneNode(true);
          form.appendChild(tokenInput);
        }

        form.appendChild(idInput);
        document.body.appendChild(form);

        // Add removal animation
        downloadItem.classList.add("row-removal");

        // Show feedback notification
        showNotification(
          `"${downloadTitle}" successfully removed from your downloads`,
          "success"
        );

        // Small delay to show the animation
        setTimeout(() => {
          form.submit();
        }, 600);
      } catch (error) {
        console.error("Error deleting download:", error);
        // Restore button state
        this.innerHTML = originalContent;
        this.disabled = false;
        downloadItem.style.opacity = "1";
        downloadItem.style.pointerEvents = "auto";

        // Show error notification
        showNotification(
          "Failed to delete the download. Please try again.",
          "error"
        );
      }
    });
  });
}

// Setup subscription management
function setupSubscriptionManagement() {
  // Handle manage subscription button actions
  document
    .querySelectorAll('form[action*="CancelSubscription"]')
    .forEach((form) => {
      form.addEventListener("submit", function (e) {
        const subscriptionType = this.querySelector(
          'input[name="subscriptionType"]'
        ).value;

        if (
          !confirm(
            `Are you sure you want to cancel your ${subscriptionType} subscription? You'll lose access to premium features.`
          )
        ) {
          e.preventDefault();
          return false;
        }

        // Show loading state
        const button = this.querySelector('button[type="submit"]');
        const originalText = button.innerHTML;
        button.disabled = true;
        button.innerHTML =
          '<i class="fas fa-circle-notch fa-spin mr-2"></i> Processing...';

        // Let the form submission continue
        return true;
      });
    });

  // Add renewal confirmation
  document
    .querySelectorAll('form[action*="RenewSubscription"]')
    .forEach((form) => {
      form.addEventListener("submit", function (e) {
        const subscriptionType = this.querySelector(
          'input[name="subscriptionType"]'
        ).value;

        if (
          !confirm(
            `Renew your ${subscriptionType} subscription for another year?`
          )
        ) {
          e.preventDefault();
          return false;
        }

        // Show loading state
        const button = this.querySelector('button[type="submit"]');
        const originalText = button.innerHTML;
        button.disabled = true;
        button.innerHTML =
          '<i class="fas fa-circle-notch fa-spin mr-2"></i> Processing...';
      });
    });

  // Handle subscription success (when redirected back with success message)
  if (window.location.search.includes("subscribed=true")) {
    // Find the tab button for subscriptions
    setTimeout(() => {
      activateTab("subscriptions");

      // Find newly subscribed card and add animation
      const subscriptionType = new URLSearchParams(window.location.search).get(
        "type"
      );
      if (subscriptionType) {
        const isRenewal = window.location.search.includes("renewed=true");
        const message = isRenewal
          ? `Your ${subscriptionType} subscription has been successfully renewed!`
          : `You have successfully subscribed to ${subscriptionType}!`;

        // Create a notification for the user
        createTemporaryNotification(message, "success");

        const cardClass = subscriptionType.toLowerCase().includes("google")
          ? "subscription-pulse"
          : "amber-subscription-pulse";

        // Find and animate the corresponding subscription row
        const subscriptionRows = document.querySelectorAll(".subscription-row");
        subscriptionRows.forEach((row) => {
          if (row.textContent.includes(subscriptionType)) {
            row.classList.add("subscription-success");
            setTimeout(() => {
              row.scrollIntoView({ behavior: "smooth", block: "center" });
            }, 300);
          }
        });

        // Find and animate the corresponding subscription badge
        const badges = document.querySelectorAll(".active-subscription-badge");
        badges.forEach((badge) => {
          if (badge.textContent.includes(subscriptionType)) {
            badge.classList.add("subscription-badge-pulse");
          }
        });
      }
    }, 500);
  }

  // Handle subscription expiry warnings
  document.querySelectorAll(".subscription-expiry").forEach((element) => {
    const daysText = element.textContent;
    const daysMatch = daysText.match(/Expires in (\d+) days/);
    if (daysMatch) {
      const days = parseInt(daysMatch[1]);
      if (days <= 3) {
        element.classList.add("expiration-critical");
      } else if (days <= 7) {
        element.classList.add("expiration-warning");
      }
    }
  });
}

// Setup search functionality for saved books
function setupBookSearch() {
  const searchInput = document.getElementById("bookSearch");
  const clearSearchBtn = document.getElementById("clearSearch");
  const bookCards = document.querySelectorAll(".book-card");
  const booksListView = document.getElementById("booksListView");

  if (!searchInput || !bookCards.length || !booksListView) {
    return; // Exit if elements don't exist
  }

  // Debounce function to improve performance
  function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  }

  // Search function
  function performSearch() {
    const searchTerm = searchInput.value.trim().toLowerCase();
    let hasResults = false;
    let visibleCount = 0;

    // Show/hide clear button
    if (searchTerm.length > 0) {
      clearSearchBtn.classList.remove("hidden");
      clearSearchBtn.classList.add("flex");
    } else {
      clearSearchBtn.classList.add("hidden");
      clearSearchBtn.classList.remove("flex");
    }

    // Filter books
    bookCards.forEach((card) => {
      const title = card.querySelector("h3")?.textContent.toLowerCase() || "";
      const author =
        card
          .querySelector(".text-sm.text-gray-600")
          ?.textContent.toLowerCase() || "";
      const description = card.querySelector(".book-description")
        ? card.querySelector(".book-description").textContent.toLowerCase()
        : "";
      const genres =
        card.querySelector(".book-genres")?.textContent.toLowerCase() || "";

      // Check if the card content matches the search term
      if (
        title.includes(searchTerm) ||
        author.includes(searchTerm) ||
        description.includes(searchTerm) ||
        genres.includes(searchTerm)
      ) {
        card.classList.remove("hidden");
        card.style.display = "";
        hasResults = true;
        visibleCount++;

        // Add highlight to matched text if search term exists
        if (searchTerm && searchTerm.length > 2) {
          highlightText(card, searchTerm);
        } else {
          removeHighlights(card);
        }
      } else {
        card.classList.add("hidden");
        card.style.display = "none";
        removeHighlights(card);
      }
    });

    // Display message if no results found
    const noResultsMsg = document.getElementById("noSearchResults");
    if (!hasResults && searchTerm.length > 0) {
      if (!noResultsMsg) {
        const msgElement = document.createElement("div");
        msgElement.id = "noSearchResults";
        msgElement.className =
          "text-center py-8 bg-gray-50 rounded-lg border border-gray-200 animate-fade-in";
        msgElement.innerHTML = `
          <div class="mx-auto h-16 w-16 text-gray-400 mb-4 flex items-center justify-center">
            <i class="fas fa-search fa-2x"></i>
          </div>
          <h3 class="text-lg font-medium text-gray-900">No books found</h3>
          <p class="mt-2 text-sm text-gray-500">
            No books match your search for "<span class="font-medium text-primary-600">${searchTerm}</span>".
          </p>
          <button class="mt-4 px-4 py-2 bg-primary-100 text-primary-700 rounded-md hover:bg-primary-200 transition-colors duration-200">
            <i class="fas fa-times mr-1"></i> Clear Search
          </button>
        `;
        booksListView.appendChild(msgElement);

        // Add event listener to the clear button in the no results message
        msgElement
          .querySelector("button")
          .addEventListener("click", function () {
            searchInput.value = "";
            performSearch();
            searchInput.focus();
          });
      } else {
        noResultsMsg.querySelector(
          ".font-medium.text-primary-600"
        ).textContent = searchTerm;
        noResultsMsg.classList.remove("hidden");
      }
    } else if (noResultsMsg) {
      noResultsMsg.classList.add("hidden");
    }

    // Update counts if there's a counter element
    const resultsCounter = document.getElementById("searchResultsCount");
    if (resultsCounter && searchTerm.length > 0) {
      resultsCounter.textContent = `${visibleCount} result${
        visibleCount !== 1 ? "s" : ""
      }`;
      resultsCounter.classList.remove("hidden");
    } else if (resultsCounter) {
      resultsCounter.classList.add("hidden");
    }
  }

  // Function to highlight matching text
  function highlightText(card, term) {
    removeHighlights(card);

    const textElements = [
      { el: card.querySelector("h3"), className: "highlight-title" },
      {
        el: card.querySelector(".text-sm.text-gray-600"),
        className: "highlight-author",
      },
      {
        el: card.querySelector(".book-description"),
        className: "highlight-desc",
      },
    ];

    textElements.forEach(({ el, className }) => {
      if (!el) return;

      const text = el.textContent;
      const regex = new RegExp(`(${term})`, "gi");

      if (regex.test(text)) {
        el.innerHTML = text.replace(
          regex,
          `<span class="bg-yellow-100 rounded px-0.5">$1</span>`
        );
      }
    });
  }

  // Function to remove highlights
  function removeHighlights(card) {
    const elements = [
      card.querySelector("h3"),
      card.querySelector(".text-sm.text-gray-600"),
      card.querySelector(".book-description"),
    ];

    elements.forEach((el) => {
      if (el && el.querySelector("span.bg-yellow-100")) {
        el.innerHTML = el.textContent;
      }
    });
  }

  // Set up event listeners with debounce for performance
  searchInput.addEventListener("input", debounce(performSearch, 300));

  // Initial search in case there's a pre-filled value
  if (searchInput.value) {
    performSearch();
  }

  // Clear search button
  if (clearSearchBtn) {
    clearSearchBtn.addEventListener("click", function () {
      searchInput.value = "";
      performSearch();
      searchInput.focus();
    });
  }

  // Add keyboard shortcut (Ctrl+F or Cmd+F) to focus the search input
  document.addEventListener("keydown", function (e) {
    if (
      (e.ctrlKey || e.metaKey) &&
      e.key === "f" &&
      document.getElementById("savedBooks") &&
      !document.getElementById("savedBooks").classList.contains("hidden")
    ) {
      e.preventDefault();
      searchInput.focus();
    }
  });
}

// Helper function to create temporary notification
function createTemporaryNotification(message, type = "info") {
  const container = document.getElementById("notification-container");
  if (!container) return;

  const notificationTypes = {
    success: {
      icon: "fas fa-check-circle",
      bgColor: "bg-green-50",
      borderColor: "border-green-400",
      textColor: "text-green-800",
      iconColor: "text-green-500",
    },
    info: {
      icon: "fas fa-info-circle",
      bgColor: "bg-blue-50",
      borderColor: "border-blue-400",
      textColor: "text-blue-800",
      iconColor: "text-blue-500",
    },
    warning: {
      icon: "fas fa-exclamation-circle",
      bgColor: "bg-yellow-50",
      borderColor: "border-yellow-400",
      textColor: "text-yellow-800",
      iconColor: "text-yellow-500",
    },
  };

  const style = notificationTypes[type];

  const notification = document.createElement("div");
  notification.className = `mb-6 rounded-lg ${style.bgColor} p-4 border-l-4 ${style.borderColor} transition-all duration-500 transform animate-fade-in shadow-md`;
  notification.innerHTML = `
    <div class="flex items-center">
      <div class="flex-shrink-0">
        <i class="${style.icon} ${style.iconColor} text-lg"></i>
      </div>
      <div class="ml-3">
        <p class="text-sm font-medium ${style.textColor}">${message}</p>
      </div>
      <div class="ml-auto pl-3">
        <div class="-mx-1.5 -my-1.5">
          <button onclick="this.parentElement.parentElement.parentElement.parentElement.remove()"
                  class="inline-flex rounded-md p-1.5 ${
                    style.iconColor
                  } hover:${style.bgColor.replace(
    "50",
    "100"
  )} focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-${style.borderColor
    .replace("border-", "")
    .replace("-400", "-500")} transition-all duration-200">
            <span class="sr-only">Dismiss</span>
            <i class="fas fa-times"></i>
          </button>
        </div>
      </div>
    </div>
  `;

  container.prepend(notification);

  // Auto-remove after 8 seconds
  setTimeout(() => {
    notification.style.opacity = "0";
    notification.style.transform = "translateY(-20px)";
    setTimeout(() => notification.remove(), 500);
  }, 8000);
}

// Notification display function
function showNotification(message, type = "success") {
  const container = document.getElementById("notification-container");
  if (!container) return;

  const notification = document.createElement("div");
  notification.className = `rounded-lg p-4 shadow-md transition-all duration-300 transform opacity-0 translate-y-2 mb-4 ${
    type === "success"
      ? "bg-green-50 border-l-4 border-green-400"
      : "bg-red-50 border-l-4 border-red-400"
  }`;

  notification.innerHTML = `
        <div class="flex">
            <div class="flex-shrink-0">
                <i class="fas ${
                  type === "success"
                    ? "fa-check-circle text-green-500"
                    : "fa-exclamation-circle text-red-500"
                } text-lg"></i>
            </div>
            <div class="ml-3">
                <p class="text-sm font-medium ${
                  type === "success" ? "text-green-800" : "text-red-800"
                }">
                    ${message}
                </p>
            </div>
            <div class="ml-auto pl-3">
                <div class="-mx-1.5 -my-1.5">
                    <button type="button" 
                            class="inline-flex rounded-md p-1.5 
                            ${
                              type === "success"
                                ? "text-green-500 hover:bg-green-100 focus:ring-green-500"
                                : "text-red-500 hover:bg-red-100 focus:ring-red-500"
                            } 
                            focus:outline-none focus:ring-2 focus:ring-offset-2">
                        <span class="sr-only">Dismiss</span>
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            </div>
        </div>
    `;

  container.prepend(notification);

  // Animate in
  setTimeout(() => {
    notification.style.opacity = "1";
    notification.style.transform = "translateY(0)";
  }, 10);

  // Add click handler for close button
  notification.querySelector("button").addEventListener("click", () => {
    notification.style.opacity = "0";
    notification.style.transform = "translateY(-10px)";
    setTimeout(() => {
      notification.remove();
    }, 300);
  });

  // Auto dismiss after 5 seconds
  setTimeout(() => {
    if (notification.parentNode) {
      notification.style.opacity = "0";
      notification.style.transform = "translateY(-10px)";
      setTimeout(() => {
        if (notification.parentNode) notification.remove();
      }, 300);
    }
  }, 5000);
}
