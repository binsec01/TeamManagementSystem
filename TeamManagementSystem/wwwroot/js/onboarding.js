(function () {
    const steps = [
        document.getElementById("onboarding-step-1"),
        document.getElementById("onboarding-step-2"),
        document.getElementById("onboarding-step-3"),
        document.getElementById("onboarding-step-4")
    ];

    if (!steps[0]) {
        return;
    }

    let currentStep = 0;
    const stepLabel = document.getElementById("onboarding-step-label");
    const backBtn = document.getElementById("onboarding-back");
    const nextBtn = document.getElementById("onboarding-next");
    const skipBtn = document.getElementById("onboarding-skip");

    const orgIdInput = document.getElementById("onboarding-organization-id");
    const teamIdInput = document.getElementById("onboarding-team-id");
    const projectIdInput = document.getElementById("onboarding-project-id");

    const orgNameInput = document.getElementById("org-name");
    const orgIndustryInput = document.getElementById("org-industry");
    const projectNameInput = document.getElementById("project-name");
    const projectDescriptionInput = document.getElementById("project-description");
    const selectedTemplateInput = document.getElementById("selected-template");

    const inviteList = document.getElementById("invite-list");
    const addInviteRowBtn = document.getElementById("add-invite-row");

    function updateStepUi() {
        steps.forEach((s, index) => {
            if (index === currentStep) {
                s.classList.remove("d-none");
                s.classList.add("active");
            } else {
                s.classList.add("d-none");
                s.classList.remove("active");
            }
        });
        stepLabel.textContent = "Step " + (currentStep + 1) + " of 4";
        backBtn.disabled = currentStep === 0;
        skipBtn.style.display = currentStep === 3 ? "inline-block" : "none";
        validateCurrentStep();
    }

    function validateCurrentStep() {
        let valid = true;
        if (currentStep === 0) {
            valid = orgNameInput.value.trim().length > 0;
        } else if (currentStep === 1) {
            valid = projectNameInput.value.trim().length > 0;
        } else if (currentStep === 2) {
            valid = selectedTemplateInput.value.trim().length > 0;
        }
        nextBtn.disabled = !valid;
    }

    function showError(message) {
        alert(message || "Something went wrong. Please try again.");
    }

    function postJson(url, data) {
        return fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": getRequestVerificationToken()
            },
            body: JSON.stringify(data)
        }).then(function (response) {
            if (!response.ok) {
                throw new Error("Request failed");
            }
            const contentType = response.headers.get("content-type") || "";
            if (contentType.indexOf("application/json") !== -1) {
                return response.json();
            }
            return null;
        });
    }

    function getRequestVerificationToken() {
        const tokenInput = document.querySelector("input[name='__RequestVerificationToken']");
        return tokenInput ? tokenInput.value : "";
    }

    function handleNext() {
        if (currentStep === 0) {
            nextBtn.disabled = true;
            postJson("/Onboarding/CreateOrganization", {
                name: orgNameInput.value.trim(),
                industry: orgIndustryInput.value.trim(),
                defaultTeamName: "General"
            }).then(function (data) {
                orgIdInput.value = data.organizationId;
                teamIdInput.value = data.teamId;
                currentStep = 1;
                updateStepUi();
            }).catch(function () {
                showError();
            }).finally(function () {
                nextBtn.disabled = false;
            });
        } else if (currentStep === 1) {
            nextBtn.disabled = true;
            postJson("/Onboarding/CreateProject", {
                organizationId: parseInt(orgIdInput.value, 10),
                teamId: parseInt(teamIdInput.value, 10),
                name: projectNameInput.value.trim(),
                description: projectDescriptionInput.value.trim()
            }).then(function (data) {
                projectIdInput.value = data.projectId;
                currentStep = 2;
                updateStepUi();
            }).catch(function () {
                showError();
            }).finally(function () {
                nextBtn.disabled = false;
            });
        } else if (currentStep === 2) {
            nextBtn.disabled = true;
            postJson("/Onboarding/ApplyTemplate", {
                projectId: parseInt(projectIdInput.value, 10),
                templateKey: selectedTemplateInput.value.trim()
            }).then(function () {
                currentStep = 3;
                updateStepUi();
            }).catch(function () {
                showError();
            }).finally(function () {
                nextBtn.disabled = false;
            });
        } else if (currentStep === 3) {
            finishOnboarding(false);
        }
    }

    function collectInvites() {
        const rows = inviteList.querySelectorAll(".invite-row");
        const invites = [];
        rows.forEach(function (row) {
            const emailInput = row.querySelector(".invite-email");
            const roleSelect = row.querySelector(".invite-role");
            const email = emailInput.value.trim();
            if (email.length > 0) {
                invites.push({
                    email: email,
                    role: roleSelect.value
                });
            }
        });
        return invites;
    }

    function finishOnboarding(skipInvites) {
        const projectId = parseInt(projectIdInput.value, 10);
        if (!projectId) {
            showError("Project was not created correctly.");
            return;
        }

        if (skipInvites) {
            window.location.href = "/Tasks/Board?projectId=" + projectId;
            return;
        }

        const invites = collectInvites();
        nextBtn.disabled = true;
        postJson("/Onboarding/InviteTeam", {
            projectId: projectId,
            invites: invites
        }).then(function () {
            window.location.href = "/Tasks/Board?projectId=" + projectId;
        }).catch(function () {
            showError();
        }).finally(function () {
            nextBtn.disabled = false;
        });
    }

    backBtn.addEventListener("click", function () {
        if (currentStep === 0) return;
        currentStep -= 1;
        updateStepUi();
    });

    nextBtn.addEventListener("click", function () {
        handleNext();
    });

    skipBtn.addEventListener("click", function () {
        finishOnboarding(true);
    });

    orgNameInput.addEventListener("input", validateCurrentStep);
    projectNameInput.addEventListener("input", validateCurrentStep);

    const templateButtons = document.querySelectorAll(".template-option");
    templateButtons.forEach(function (btn) {
        btn.addEventListener("click", function () {
            templateButtons.forEach(function (b) { b.classList.remove("template-selected"); });
            btn.classList.add("template-selected");
            selectedTemplateInput.value = btn.getAttribute("data-template") || "";
            validateCurrentStep();
        });
    });

    if (addInviteRowBtn) {
        addInviteRowBtn.addEventListener("click", function () {
            const row = document.createElement("div");
            row.className = "row g-2 invite-row mb-2";
            row.innerHTML = '' +
                '<div class="col-md-8">' +
                '  <input type="email" class="form-control invite-email" placeholder="teammate@example.com" />' +
                '</div>' +
                '<div class="col-md-4">' +
                '  <select class="form-select invite-role">' +
                '    <option value="">Role (optional)</option>' +
                '    <option value="Member">Member</option>' +
                '    <option value="Admin">Admin</option>' +
                '  </select>' +
                '</div>';
            inviteList.appendChild(row);
        });
    }

    updateStepUi();
})();

